using System;
using System.IO;
using System.Timers;
using System.Reflection;
using System.Collections.Generic;
using Eleon.Modding;
using System.Diagnostics;
using UnityEngine;


namespace LCDMagicMod
{
	public class LCDMagicNewAPI : IMod
	{
		IModApi	mAPI;

		//list of loaded player structures on the playfield
		Dictionary<int, PlayerStructure>	mPlayerStructs	=new Dictionary<int, PlayerStructure>();

		//constants from a config file
		Dictionary<string, int>			mIConstants		=new Dictionary<string, int>();
		Dictionary<string, PVector3>	mVConstants		=new Dictionary<string, PVector3>();
		Dictionary<int, string>			mIDConstants	=new Dictionary<int, string>();

		Timer	mLCDScanTimer, mContainerTimer;
		Timer	mAmmoTimer, mFridgeTimer;
		Timer	mSpecialTimer;


		bool	mbCheckBlockDestroyed;


		public void Init(IModApi modAPI)
		{
			mAPI	=modAPI;

			mAPI.Log("LCDMagicNewAPI: Init called.");

			mAPI.GameEvent							+=OnGameEvent;
			mAPI.Application.OnPlayfieldLoaded		+=OnPlayFieldLoaded;
			mAPI.Application.OnPlayfieldUnloaded	+=OnPlayFieldUnLoaded;
			mAPI.Application.GameEntered			+=OnGameEntered;

			LoadConstants();

			StartTimer(ref mLCDScanTimer, "LCDScanInterval", OnLCDScanTimer);
			StartTimer(ref mContainerTimer, "ContainerUpdateInterval", OnContainerTimer);
			StartTimer(ref mAmmoTimer, "AmmoUpdateInterval", OnAmmoTimer);
			StartTimer(ref mFridgeTimer, "FridgeUpdateInterval", OnFridgeTimer);
			StartTimer(ref mSpecialTimer, "SpecialUpdateInterval", OnSpecialTimer);
		}


		public void Game_Update()
		{
		}


		public void Shutdown()
		{
			mAPI.Log("LCDMagicNewAPI: Shutdown called.");

			mAPI.GameEvent							-=OnGameEvent;
			mAPI.Application.OnPlayfieldLoaded		-=OnPlayFieldLoaded;
			mAPI.Application.OnPlayfieldUnloaded	-=OnPlayFieldUnLoaded;
			mAPI.Application.GameEntered			-=OnGameEntered;
		}


		void OnPlayFieldUnLoaded(string pfName)
		{
			mAPI.Log("Playfield: " + pfName + " unloaded...");

			if(mAPI.Playfield == null)
			{
				mAPI.Log("Playfield is null in OnPlayFieldUnLoaded()");
				return;
			}

			if(pfName != mAPI.Playfield.Name)
			{
				//not really interested in other playfields
				return;
			}

			//unwire entity loaded so no leakage
			mAPI.Playfield.OnEntityLoaded	-=OnEntityLoaded;
			mAPI.Playfield.OnEntityUnloaded	-=OnEntityUnLoaded;
		}


		void OnPlayFieldLoaded(string pfName)
		{
			mAPI.Log("Playfield: " + pfName + " loaded...");

			if(mAPI.Playfield == null)
			{
				mAPI.Log("Playfield is null in OnPlayFieldLoaded()");
				return;
			}

			if(pfName != mAPI.Playfield.Name)
			{
				//not really interested in other playfields
				return;
			}

			mAPI.Playfield.OnEntityLoaded	+=OnEntityLoaded;
			mAPI.Playfield.OnEntityUnloaded	+=OnEntityUnLoaded;
		}


		void OnGameEntered(bool bSomething)
		{
			mAPI.Log("OnGameEntered(): " + bSomething);
		}


		void OnGameEvent(GameEventType gvt, object stuff0, object stuff1, object stuff2, object stuff3, object stuff4)
		{
			mAPI.Log("Game Event: " + gvt);

			if(gvt == GameEventType.BlockDestroyed)
			{
				mAPI.Log("Block Changed: " + stuff0 + ", " + stuff1 + ", " + stuff2 + ", " + stuff3 + ", " + stuff4 + "...");

				mbCheckBlockDestroyed	=true;
			}
		}


		void OnEntityUnLoaded(IEntity ent)
		{
			if(mPlayerStructs.ContainsKey(ent.Id))
			{
				mAPI.Log("Unloaded Entity: " + ent.Id + " being removed from structure list...");

				mPlayerStructs[ent.Id].UnLoad();

				mPlayerStructs.Remove(ent.Id);
			}
		}


		void OnEntityLoaded(IEntity ent)
		{
			mAPI.Log("Entity: " + ent.Id + ", loaded...");
			mAPI.Log("FactionData: " + ent.Faction);
			mAPI.Log("ForwardVec: " + ent.Forward);
			mAPI.Log("Name: " + ent.Name);
			mAPI.Log("Structure: " + ent.Structure);

			if(ent.Structure != null)
			{
				mAPI.Log("Structure FactionData.Id: " + ent.Faction.Id);
				mAPI.Log("Structure FactionData.Group: " + ent.Faction.Group);

				if(ent.Faction.Group == FactionGroup.Player
					|| ent.Faction.Group == FactionGroup.Faction)
				{
					mAPI.Log("Keeping ref to player structure...");

					if(!mPlayerStructs.ContainsKey(ent.Id))
					{
						mPlayerStructs.Add(ent.Id, new PlayerStructure(ent.Structure));
					}
				}
			}
		}


		void OnSpecialTimer(object sender, ElapsedEventArgs eea)
		{
			foreach(KeyValuePair<int, PlayerStructure> ps in mPlayerStructs)
			{
				ps.Value.UpdateSpecial(mIDConstants);
			}
		}


		void OnAmmoTimer(object sender, ElapsedEventArgs eea)
		{
			foreach(KeyValuePair<int, PlayerStructure> ps in mPlayerStructs)
			{
				ps.Value.UpdateAmmo(mIDConstants);
			}
		}


		void OnContainerTimer(object sender, ElapsedEventArgs eea)
		{
			foreach(KeyValuePair<int, PlayerStructure> ps in mPlayerStructs)
			{
				ps.Value.UpdateContainer(mIDConstants);
			}
		}


		void OnFridgeTimer(object sender, ElapsedEventArgs eea)
		{
			foreach(KeyValuePair<int, PlayerStructure> ps in mPlayerStructs)
			{
				ps.Value.UpdateFridge(mIDConstants);
			}
		}


		void OnLCDScanTimer(object sender, ElapsedEventArgs eea)
		{
			foreach(KeyValuePair<int, PlayerStructure> ps in mPlayerStructs)
			{
				ps.Value.LCDScan(mbCheckBlockDestroyed, mAPI, mIConstants["BlockScanDistance"]);				
			}
			mbCheckBlockDestroyed	=false;
		}


		void BlockSearch(IStructure str)
		{
			//in my test case, the y is off by 128
			VectorInt3	minWorld	=new VectorInt3(
				str.MinPos.x,
				str.MinPos.y + 128,
				str.MinPos.z);

			VectorInt3	maxWorld	=new VectorInt3(
				str.MaxPos.x,
				str.MaxPos.y + 128,
				str.MaxPos.z);


			for(int x=minWorld.x;x <= maxWorld.x;x++)
			{
				for(int y=minWorld.y;y <= maxWorld.y;y++)
				{
					for(int z=minWorld.z;z <= maxWorld.z;z++)
					{
						IBlock	bl	=str.GetBlock(x, y, z);

						mAPI.Log("Block at " + x + ", " + y + ", " + z +
								" custom name: " + bl.CustomName +
								", Damage: " + bl.GetDamage() +
								", HitPoints: " + bl.GetHitPoints());
					}
				}
			}
		}


		void StartTimer(ref Timer t, string intervalKey, ElapsedEventHandler elapsed)
		{
			t			=new Timer(mIConstants[intervalKey]);
			t.AutoReset	=true;
			t.Elapsed	+=elapsed;
			t.Start();
		}


 		void LoadConstants()
		{
			//can't be too sure of what the current directory is
			Assembly	ass	=Assembly.GetExecutingAssembly();

			string	dllDir	=Path.GetDirectoryName(ass.Location);

			string	filePath	=Path.Combine(dllDir, "Constants.txt");

			mAPI.Log("Loading Config file for constants: " + filePath);

			FileStream	fs	=new FileStream(filePath, FileMode.Open, FileAccess.Read);
			if(fs == null)
			{
				return;
			}

			StreamReader	sr	=new StreamReader(fs);
			if(sr == null)
			{
				return;
			}

			while(!sr.EndOfStream)
			{
				string	line	=sr.ReadLine();
				if(line == "")
				{
					//skip blank lines
					continue;
				}

				string	[]toks	=line.Split(' ', '\t');

				if(toks.Length < 2)
				{
					//bad line
					mAPI.Log("Bad line in constants config file at position: " + sr.BaseStream.Position);
					continue;
				}

				//skip whitespace
				int	idx	=0;
				while(idx < toks.Length)
				{
					if(toks[idx] == "" || toks[idx] == " " || toks[idx] == "\t")
					{
						idx++;
						continue;
					}
					break;
				}

				if(toks[idx].StartsWith("//"))
				{
					continue;
				}

				//check for int values at the beginning
				{
					int	val;
					if(int.TryParse(toks[idx], out val))
					{
						if(toks.Length <= idx + 1)
						{
							continue;
						}
						mIDConstants.Add(val, toks[idx+1]);
						continue;
					}
				}

				string	cname	=toks[idx];
				idx++;

				while(idx < toks.Length)
				{
					if(toks[idx] == "" || toks[idx] == " " || toks[idx] == "\t")
					{
						idx++;
						continue;
					}
					break;
				}

				//check for vector
				if(toks[idx].StartsWith("("))
				{
					string	sansParen	=toks[idx].Substring(1);

					float	fval0	=0f;
					if(!float.TryParse(sansParen, out fval0))
					{
						mAPI.Log("Bad token:" + sansParen + ", looking for vector value in constants config file at position: " + sr.BaseStream.Position);
						continue;
					}
					idx++;

					float	fval1	=0f;
					if(!float.TryParse(toks[idx], out fval1))
					{
						mAPI.Log("Bad token:" + toks[idx] + ", looking for vector value in constants config file at position: " + sr.BaseStream.Position);
						continue;
					}
					idx++;

					sansParen	=toks[idx].Substring(0, toks[idx].Length - 1);

					float	fval2	=0f;
					if(!float.TryParse(sansParen, out fval2))
					{
						mAPI.Log("Bad token:" + sansParen + ", looking for vector value in constants config file at position: " + sr.BaseStream.Position);
						continue;
					}

					PVector3	vecVal	=new PVector3(fval0, fval1, fval2);
					mVConstants.Add(cname, vecVal);
				}
				else
				{
					int	val	=0;
					if(!int.TryParse(toks[idx], out val))
					{
						mAPI.Log("Bad token looking for value in constants config file at position: " + sr.BaseStream.Position);
						continue;
					}

					mIConstants.Add(cname, val);
				}
			}

			sr.Close();
			fs.Close();

			foreach(KeyValuePair<string, int> vals in mIConstants)
			{
				mAPI.Log("Const: " + vals.Key + ", " + vals.Value);
			}
		}
	}
}