using System;
using System.Collections.Generic;
using Eleon.Modding;
using System.Diagnostics;
using UnityEngine;


namespace LCDMagicMod
{
	internal class PlayerStructure
	{
		IStructure	mStruct;

		//devices associated with an LCD
		Dictionary<IContainer, ILcd>	mAmmoLCD		=new Dictionary<IContainer, ILcd>();
		Dictionary<IContainer, ILcd>	mContainerLCD	=new Dictionary<IContainer, ILcd>();
		Dictionary<IContainer, ILcd>	mFridgeLCD		=new Dictionary<IContainer, ILcd>();
		Dictionary<IContainer, ILcd>	mHarvestLCD		=new Dictionary<IContainer, ILcd>();

		//device positions we care about
		Dictionary<IDevice, VectorInt3>	mDevicePositions	=new Dictionary<IDevice, VectorInt3>();

		//block of the device
		Dictionary<IDevice, IBlock>	mDeviceBlocks	=new Dictionary<IDevice, IBlock>();

		//special lcds for not-yet-implemented devices and such
		List<ILcd>	mTankLCDs	=new List<ILcd>();
		List<ILcd>	mFarmLCDs	=new List<ILcd>();


		internal PlayerStructure(IStructure str)
		{
			mStruct	=str;
		}


		internal void UnLoad()
		{
			mAmmoLCD.Clear();
			mContainerLCD.Clear();
			mFridgeLCD.Clear();
			mHarvestLCD.Clear();
			mDevicePositions.Clear();
			mDeviceBlocks.Clear();
		}


		internal void UpdateSpecial(Dictionary<int, string> idToItemName)
		{
			foreach(ILcd lcd in mTankLCDs)
			{
				if(!mDeviceBlocks.ContainsKey(lcd))
				{
					continue;
				}
				IBlock	blk	=mDeviceBlocks[lcd];

				//check name for variables
				int	columnWidth	=-1;
				CheckCustomNames(lcd, out columnWidth);

				string	t	="";
				if(mStruct.FuelTank == null)
				{
					t	+="Structure has no fuel tank!\n\n";
				}
				else
				{
					t	="Power On: " + mStruct.IsPowered + "\n\n";
					t	+="Fuel Capacity: " + mStruct.FuelTank.Capacity + "\n";
					t	+="Fuel Remaining: " + mStruct.FuelTank.Content + "\n\n";
				}

				if(mStruct.OxygenTank == null)
				{
					t	+="Structure has no Oxygen tank!\n\n";
				}
				else
				{
					t	+="O2 Capacity: " + mStruct.OxygenTank.Capacity + "\n";
					t	+="O2 Remaining: " + mStruct.OxygenTank.Content + "\n";
				}

				lcd.SetText(t);
			}

			foreach(ILcd lcd in mFarmLCDs)
			{
				if(!mDeviceBlocks.ContainsKey(lcd))
				{
					continue;
				}
				IBlock	blk	=mDeviceBlocks[lcd];

				//check name for variables
				int	columnWidth	=-1;
				CheckCustomNames(lcd, out columnWidth);
				
				//not done yet
				string	t	="Number of Farm Plots: ???\n\n";
				t			+="Grow light energy load: ???\n";
				t			+="Plot x,y growth %???\n";

				lcd.SetText(t);
			}
		}


		internal void UpdateAmmo(Dictionary<int, string> idToItemName)
		{
			foreach(KeyValuePair<IContainer, ILcd> dlcd in mAmmoLCD)
			{
				//check name for variables
				int	columnWidth	=-1;
				CheckCustomNames(dlcd.Value, out columnWidth);

				string	t	="Ammo Remaining:\n";

				List<ItemStack>	stuff	=dlcd.Key.GetContent();

				bool	bColToggle	=false;
				foreach(ItemStack stk in stuff)
				{
					if(idToItemName.ContainsKey(stk.id))
					{
						AddTextLine(ref t, "Ammo: " + idToItemName[stk.id] + " " + stk.count,
									columnWidth, ref bColToggle);
					}
					else
					{
						AddTextLine(ref t, "Other Item: " + stk.id + " " + stk.count,
									columnWidth, ref bColToggle);
					}
				}

				if(!bColToggle)
				{
					t	+="\n";
				}
				else
				{
					t	+="\n\n";
				}

				t	+="Volume Capacity: " + dlcd.Key.VolumeCapacity;

				dlcd.Value.SetText(t);
			}
		}


		internal void UpdateContainer(Dictionary<int, string> idToItemName)
		{
			foreach(KeyValuePair<IContainer, ILcd> dlcd in mContainerLCD)
			{
				//check name for variables
				int	columnWidth	=-1;
				CheckCustomNames(dlcd.Value, out columnWidth);

				string	t	="Container Items:\n";

				List<ItemStack>	stuff	=dlcd.Key.GetContent();

				bool	bColToggle	=false;
				foreach(ItemStack stk in stuff)
				{
					if(idToItemName.ContainsKey(stk.id))
					{
						AddTextLine(ref t, "Known Item: " + idToItemName[stk.id] + " " + stk.count,
									columnWidth, ref bColToggle);
					}
					else
					{
						AddTextLine(ref t, "UnKnown Item: " + stk.id + " " + stk.count,
									columnWidth, ref bColToggle);
					}
				}

				if(!bColToggle)
				{
					t	+="\n";
				}
				else
				{
					t	+="\n\n";
				}

				t	+="Volume Capacity: " + dlcd.Key.VolumeCapacity;

				dlcd.Value.SetText(t);
			}
		}


		internal void UpdateFridge(Dictionary<int, string> idToItemName)
		{
			foreach(KeyValuePair<IContainer, ILcd> dlcd in mFridgeLCD)
			{
				//check name for variables
				int	columnWidth	=-1;
				CheckCustomNames(dlcd.Value, out columnWidth);
				
				string	t	="Fridge Items:\n";

				List<ItemStack>	stuff	=dlcd.Key.GetContent();

				bool	bColToggle	=false;
				foreach(ItemStack stk in stuff)
				{
					if(idToItemName.ContainsKey(stk.id))
					{
						AddTextLine(ref t,"Known Item: " + idToItemName[stk.id] + " " + stk.count,
									columnWidth, ref bColToggle);
					}
					else
					{
						AddTextLine(ref t, "UnKnown Item: " + stk.id + " " + stk.count,
									columnWidth, ref bColToggle);
					}
				}

				if(!bColToggle)
				{
					t	+="\n";
				}
				else
				{
					t	+="\n\n";
				}

				t	+="Volume Capacity: " + dlcd.Key.VolumeCapacity;
				t	+="\nDecay Factor: " + dlcd.Key.DecayFactor;

				dlcd.Value.SetText(t);
			}
		}


		//check for newly placed or removed lcds
		//the timer for this should be biggish as all this is expensive
		internal void LCDScan(bool bCheckBlockDestroyed, IModApi api, float blockDist)
		{
			if(bCheckBlockDestroyed)
			{
				CheckAssociatedLCDs(api);
			}

			if(mStruct == null)
			{
				api.Log("Null struct in player struct list!");
				return;
			}
			if(!mStruct.IsReady)
			{
				api.Log("Struct: " + mStruct + " not ready...");
				return;
			}
			if(!mStruct.IsPowered)
			{
				//api.Log("Struct: " + mStruct + " not powered...");
				return;
			}

			/*
			string	[]dTypeNames	=mStruct.GetDeviceTypeNames();
			foreach(string dname in dTypeNames)
			{
				api.Log("Device: " + dname);
			}*/

			IDevicePosList	idplAmmo		=mStruct.GetDevices("AmmoCntr");
			IDevicePosList	idplContainer	=mStruct.GetDevices("Container");
			IDevicePosList	idplFridge		=mStruct.GetDevices("Fridge");
			IDevicePosList	idplHarvest		=mStruct.GetDevices("HarvestCntr");

			CheckLCD(api, idplAmmo, idplContainer, idplFridge, idplHarvest, blockDist);
		}


		static void AddTextLine(ref string text, string line, int columnWidth, ref bool bColumnToggle)
		{
			if(columnWidth > 0)
			{
				if(bColumnToggle)
				{
					text	+=line + "\n";
				}
				else
				{
					text	+=line.PadRight(columnWidth);
				}
				bColumnToggle	=!bColumnToggle;
			}
			else
			{
				text	+=line + "\n";
			}
		}


		static float	VecDistance(VectorInt3 a, VectorInt3 b)
		{
			int	x	=a.x - b.x;
			int	y	=a.y - b.y;
			int	z	=a.z - b.z;

			int	lenSQ	=(x * x) + (y * y) + (z * z);

			return	(float)Math.Sqrt(lenSQ);
		}


		void CheckCustomNames(ILcd lcd, out int columnWidth)
		{
			//check name for variables
			columnWidth	=-1;
			if(mDeviceBlocks.ContainsKey(lcd))
			{
				Dictionary<string, string>	vars	=new Dictionary<string, string>();

				ParseVars(mDeviceBlocks[lcd].CustomName, vars);
				
				if(vars.ContainsKey("$Fnt"))
				{
					int	fontSize;
					if(Int32.TryParse(vars["$Fnt"], out fontSize))
					{
						lcd.SetFontSize(fontSize);
					}
				}
				if(vars.ContainsKey("$CW"))
				{
					if(Int32.TryParse(vars["$CW"], out columnWidth))
					{
					}
				}
			}
		}


		void CheckForSpecialLCD(IBlock blk, ILcd lcd)
		{
			if(mTankLCDs.Contains(lcd))
			{
				return;	//already in use
			}
			if(mFarmLCDs.Contains(lcd))
			{
				return;	//already in use
			}

			Dictionary<string, string>	vars	=new Dictionary<string, string>();

			ParseVars(blk.CustomName, vars);
			if(vars.ContainsKey("$Tnk"))
			{
				//all the stuff from stats
				mTankLCDs.Add(lcd);
				mDeviceBlocks.Add(lcd, blk);
			}
			else if(vars.ContainsKey("$Frm"))
			{
				//number of plots
				//energy used by grow lights
				//growth data on individual plots
				mFarmLCDs.Add(lcd);
				mDeviceBlocks.Add(lcd, blk);
			}
		}


		void CheckLCD(IModApi api, IDevicePosList ammo, IDevicePosList container,
					IDevicePosList fridge, IDevicePosList harvest, float blockScanDist)
		{
			IDevicePosList	idpl	=mStruct.GetDevices("LCD");

			for(int i=0;i < idpl.Count;i++)
			{
				VectorInt3	pos	=idpl.GetAt(i);

				ILcd	lcd	=mStruct.GetDevice<ILcd>(pos);

				if(mAmmoLCD.ContainsValue(lcd))
				{
					continue;	//already in use
				}
				if(mContainerLCD.ContainsValue(lcd))
				{
					continue;	//already in use
				}
				if(mFridgeLCD.ContainsValue(lcd))
				{
					continue;	//already in use
				}
				if(mHarvestLCD.ContainsValue(lcd))
				{
					continue;	//already in use
				}

				api.Log("Unattached LCD Device at pos: " + pos);

				//find the closest device within BlockScanDistance
				float		blockDist	=blockScanDist;
				float		bestDist	=float.MaxValue;
				VectorInt3	bestPos		=new VectorInt3(-1, -1, -1);
				int			bestType	=-1;
				IDevice		assoc		=null;

				for(int j=0;j < ammo.Count;j++)
				{
					VectorInt3	pos2	=ammo.GetAt(j);

					float	dist	=VecDistance(pos, pos2);
					if(dist < blockDist && dist < bestDist)
					{
						assoc		=mStruct.GetDevice<IContainer>(pos2);
						bestDist	=dist;
						bestType	=0;
						bestPos		=pos2;
					}
				}

				for(int j=0;j < container.Count;j++)
				{
					VectorInt3	pos2	=container.GetAt(j);

					float	dist	=VecDistance(pos, pos2);
					if(dist < blockDist && dist < bestDist)
					{
						assoc		=mStruct.GetDevice<IContainer>(pos2);
						bestDist	=dist;
						bestType	=1;
						bestPos		=pos2;
					}
				}

				for(int j=0;j < fridge.Count;j++)
				{
					VectorInt3	pos2	=fridge.GetAt(j);

					float	dist	=VecDistance(pos, pos2);
					if(dist < blockDist && dist < bestDist)
					{
						assoc		=mStruct.GetDevice<IContainer>(pos2);
						bestDist	=dist;
						bestType	=2;
						bestPos		=pos2;
					}
				}

				for(int j=0;j < harvest.Count;j++)
				{
					VectorInt3	pos2	=harvest.GetAt(j);

					float	dist	=VecDistance(pos, pos2);
					if(dist < blockDist && dist < bestDist)
					{
						assoc		=mStruct.GetDevice<IContainer>(pos2);
						bestDist	=dist;
						bestType	=3;
						bestPos		=pos2;
					}
				}

				IBlock	lcdBlock	=mStruct.GetBlock(pos);

				if(assoc == null)
				{
					api.Log("Null Assoc");

					//maybe since this isn't near a device
					//it can be used for fuel or something
					CheckForSpecialLCD(lcdBlock, lcd);
					continue;
				}

				if(!mDevicePositions.ContainsKey(lcd))
				{
					mDevicePositions.Add(lcd, pos);
				}
				if(!mDevicePositions.ContainsKey(assoc))
				{
					mDevicePositions.Add(assoc, bestPos);
				}

				IBlock	devBlock	=mStruct.GetBlock(bestPos);

				if(lcdBlock == null)
				{
					api.Log("Null block for lcd!");
				}
				else
				{
					if(mDeviceBlocks.ContainsKey(lcd))
					{
						api.Log("BadCleanup!  Device blocks already has lcd: " + lcd + "!!");
					}
					else
					{
						mDeviceBlocks.Add(lcd, lcdBlock);
					}
				}

				if(devBlock == null)
				{
					api.Log("Null block for device!");
				}
				else
				{
					if(mDeviceBlocks.ContainsKey(assoc))
					{
						api.Log("BadCleanup!  Device blocks already has assoc: " + assoc + "!!");
					}
					else
					{
						mDeviceBlocks.Add(assoc, devBlock);
					}
				}

				if(bestType == 0)
				{
					api.Log("Ammo Assoc");
					mAmmoLCD.Add(assoc as IContainer, lcd);
				}
				else if(bestType == 1)
				{
					api.Log("Con Assoc");
					mContainerLCD.Add(assoc as IContainer, lcd);
				}
				else if(bestType == 2)
				{
					api.Log("Fridge Assoc");
					mFridgeLCD.Add(assoc as IContainer, lcd);
				}
				else if(bestType == 3)
				{
					api.Log("Harv Assoc");
					mHarvestLCD.Add(assoc as IContainer, lcd);
				}
			}
		}


		void NukeLCD(Dictionary<IContainer, ILcd> dict, ILcd toNuke)
		{
			IContainer	found	=null;
			foreach(KeyValuePair<IContainer, ILcd> clcd in dict)
			{
				if(clcd.Value == toNuke)
				{
					found	=clcd.Key;
					break;
				}
			}
			if(found != null)
			{
				dict.Remove(found);
				mDevicePositions.Remove(toNuke);
				mDevicePositions.Remove(found);
				mDeviceBlocks.Remove(toNuke);
				mDeviceBlocks.Remove(found);
			}
		}


		void NukeLCD(List<ILcd> lcds, ILcd toNuke)
		{
			if(lcds.Contains(toNuke))
			{
				lcds.Remove(toNuke);
				mDevicePositions.Remove(toNuke);
				mDeviceBlocks.Remove(toNuke);
			}
		}


		//normally called when a block is deleted
		void CheckAssociatedLCDs(IModApi api)
		{
			List<IDevice>	toNuke	=new List<IDevice>();

			//check ammo
			ConfirmStillThere(mAmmoLCD, toNuke);

			//check container
			ConfirmStillThere(mContainerLCD, toNuke);

			//check fridge
			ConfirmStillThere(mFridgeLCD, toNuke);

			//check harvest
			ConfirmStillThere(mHarvestLCD, toNuke);

			//check tanks
			ConfirmStillThere(mTankLCDs, toNuke);

			//check farm
			ConfirmStillThere(mFarmLCDs, toNuke);

			if(toNuke.Count <= 0)
			{
				return;
			}

			api.Log("CheckAssociatedLCDs found " + toNuke.Count + " nuked items...");

			for(int i=0;i < toNuke.Count;i++)
			{
				if(toNuke[i] is ILcd)
				{
					ILcd	lcd	=toNuke[i] as ILcd;

					NukeLCD(mAmmoLCD, lcd);
					NukeLCD(mContainerLCD, lcd);
					NukeLCD(mFridgeLCD, lcd);
					NukeLCD(mHarvestLCD, lcd);
					NukeLCD(mTankLCDs, lcd);
					NukeLCD(mFarmLCDs, lcd);
				}
				else if(toNuke[i] is IContainer)
				{
					IContainer	con	=toNuke[i] as IContainer;
					ILcd		lcd	=null;

					if(mAmmoLCD.ContainsKey(con))
					{
						lcd	=mAmmoLCD[con];
						mAmmoLCD.Remove(con);
					}
					if(mContainerLCD.ContainsKey(con))
					{
						lcd	=mContainerLCD[con];
						mContainerLCD.Remove(con);
					}
					if(mFridgeLCD.ContainsKey(con))
					{
						lcd	=mFridgeLCD[con];
						mFridgeLCD.Remove(con);
					}
					if(mHarvestLCD.ContainsKey(con))
					{
						lcd	=mHarvestLCD[con];
						mHarvestLCD.Remove(con);
					}

					//make sure if one device is gone
					//to remove the associated stuff as well
					if(lcd != null)
					{
						mDevicePositions.Remove(lcd);
						mDeviceBlocks.Remove(lcd);
						mDevicePositions.Remove(con);
						mDeviceBlocks.Remove(con);
					}
				}
			}
		}


		void	ConfirmStillThere(Dictionary<IContainer, ILcd> dict, List<IDevice> toNuke)
		{
			foreach(KeyValuePair<IContainer, ILcd> clcd in dict)
			{
				//check container
				IContainer	cn	=mStruct.GetDevice<IContainer>(mDevicePositions[clcd.Key]);
				if(cn != clcd.Key)
				{
					toNuke.Add(clcd.Key);
				}

				//check lcd
				ILcd	lcd	=mStruct.GetDevice<ILcd>(mDevicePositions[clcd.Value]);
				if(lcd != clcd.Value)
				{
					toNuke.Add(clcd.Value);
				}
			}
		}


		void	ConfirmStillThere(List<ILcd> lcds, List<IDevice> toNuke)
		{
			foreach(ILcd lcd in lcds)
			{
				//check lcd
				ILcd	lcdCheck	=mStruct.GetDevice<ILcd>(mDevicePositions[lcd]);
				if(lcd != lcdCheck)
				{
					toNuke.Add(lcd);
				}
			}
		}


		void ParseVars(string name, Dictionary<string, string> vars)
		{
			int	firstDollar	=name.IndexOf('$');
			if(firstDollar < 0)
			{
				return;
			}

			int	nextDollar	=name.IndexOf('$', firstDollar + 1);
			int	nextEqual	=name.IndexOf('=', firstDollar + 1);

			string	key	="";
			string	val	="";

			if(nextDollar <=0 && nextEqual <= 0)
			{
				key	=name.Substring(firstDollar);

				vars.Add(key, "");
				return;
			}

			if(nextDollar > 0 && nextDollar < nextEqual)
			{
				key	=name.Substring(firstDollar, nextDollar - firstDollar);
			}
			else if(nextEqual > 0 && nextEqual < nextDollar)
			{
				key	=name.Substring(firstDollar, nextEqual - firstDollar);
				val	=name.Substring(nextEqual + 1, nextDollar - nextEqual - 1);
			}
			else if(nextEqual > 0)
			{
				key	=name.Substring(firstDollar, nextEqual - firstDollar);
				val	=name.Substring(nextEqual + 1);
			}
			else
			{
				key	=name.Substring(firstDollar, nextDollar - firstDollar);
			}

			vars.Add(key, val);

			if(nextDollar > 0)
			{
				name	=name.Substring(nextDollar);
				ParseVars(name, vars);
			}
		}
	}
}