using TMPro;
using UnityEngine;

public class NPCMapAgent
{
	public class npcDesire
	{
		public NPCSchedual.Locations desiredLocation;

		private NPCMapAgent myMapAgent;

		private int npcId = -1;

		public NPCBuildingDoors lastWalkedInto;

		public NPCBuildingDoors wantToWalkInto;

		public Transform following;

		public Vector3 desiredPos;

		public npcDesire(int newNpcId, NPCMapAgent newMapAgent)
		{
			myMapAgent = newMapAgent;
			npcId = newNpcId;
		}

		public void checkDesire(NPCAI NPCai)
		{
			if (RealWorldTimeLight.time.currentHour != 24)
			{
				desiredLocation = NPCManager.manage.NPCDetails[npcId].mySchedual.getDesiredLocation(npcId, RealWorldTimeLight.time.currentHour);
			}
			else
			{
				desiredLocation = NPCSchedual.Locations.Wonder;
			}
			desiredPos = myMapAgent.getPositionForLiveAgent();
			checkIfComplete(NPCai);
		}

		public void checkDesire(NPCAI NPCai, int hour)
		{
			if (RealWorldTimeLight.time.currentHour != 24)
			{
				desiredLocation = NPCManager.manage.NPCDetails[npcId].mySchedual.getDesiredLocation(npcId, hour);
			}
			else
			{
				desiredLocation = NPCSchedual.Locations.Wonder;
			}
			desiredPos = myMapAgent.getPositionForLiveAgent();
			checkIfComplete(NPCai);
		}

		private void checkIfComplete(NPCAI NPCai)
		{
			if (NPCai != null)
			{
				if (desiredLocation == NPCSchedual.Locations.Exit || ((bool)lastWalkedInto && lastWalkedInto != TownManager.manage.allShopFloors[(int)desiredLocation]))
				{
					if ((bool)lastWalkedInto)
					{
						if ((bool)NPCai && NPCai.talkingTo == 0 && Vector3.Distance(NPCai.transform.position, lastWalkedInto.inside.position) < 1.5f)
						{
							if (!RealWorldTimeLight.time.underGround && NPCai.checkPositionIsOnNavmesh(lastWalkedInto.outside.position))
							{
								NPCai.myAgent.Warp(lastWalkedInto.outside.position);
								NPCai.transform.position = NPCai.myAgent.transform.position;
							}
							else
							{
								NPCai.myAgent.enabled = false;
								NPCai.myAgent.transform.position = lastWalkedInto.outside.position;
								NPCai.transform.position = NPCai.myAgent.transform.position;
								NetworkNavMesh.nav.UnSpawnNPCOnTile(NPCai);
							}
							lastWalkedInto = null;
							desiredLocation = NPCSchedual.Locations.Wonder;
						}
					}
					else
					{
						desiredLocation = NPCSchedual.Locations.Wonder;
					}
				}
				else if ((bool)wantToWalkInto)
				{
					if ((bool)NPCai && NPCai.talkingTo == 0 && Vector3.Distance(NPCai.transform.position, wantToWalkInto.outside.position) < 2f)
					{
						NPCai.myAgent.Warp(wantToWalkInto.inside.position);
						NPCai.transform.position = NPCai.myAgent.transform.position;
						lastWalkedInto = wantToWalkInto;
						wantToWalkInto = null;
						desiredLocation = NPCSchedual.Locations.Wonder;
					}
				}
				else
				{
					Vector3.Distance(NPCai.transform.position, new Vector3(desiredPos.x, WorldManager.manageWorld.heightMap[Mathf.RoundToInt(desiredPos.x / 2f), Mathf.RoundToInt(desiredPos.z / 2f)], desiredPos.z * 2f));
					float num = 4f;
				}
			}
			else if (Vector3.Distance(myMapAgent.currentPosition, desiredPos) <= 5f && (bool)wantToWalkInto)
			{
				NetworkNavMesh.nav.SpawnAnNPCFromMapToPlaceInBuilding(npcId, wantToWalkInto.inside.position);
				lastWalkedInto = wantToWalkInto;
				wantToWalkInto = null;
				desiredLocation = NPCSchedual.Locations.Wonder;
			}
		}

		public void warpNpcToDesiredPos(NPCAI NPCai)
		{
			if ((bool)NPCai)
			{
				NPCai.myAgent.Warp(wantToWalkInto.inside.position);
				if (NPCai.myAgent.isActiveAndEnabled && NPCai.myAgent.isOnNavMesh)
				{
					NPCai.myAgent.SetDestination(wantToWalkInto.inside.position + wantToWalkInto.inside.forward * 2.5f);
				}
				else
				{
					NPCai.myAgent.transform.position = wantToWalkInto.inside.position;
				}
				NPCai.transform.position = NPCai.myAgent.transform.position;
				lastWalkedInto = wantToWalkInto;
				wantToWalkInto = null;
				desiredLocation = NPCSchedual.Locations.Wonder;
			}
		}
	}

	public int npcId;

	public NPCAI activeNPC;

	private npcDesire desire;

	private float mapMoveTimer;

	public Vector3 currentPosition;

	private Vector3 currentlyMovingTo;

	public GameObject debugMarker;

	public NPCMapAgent(int npcNo, int startingX, int startingY)
	{
		npcId = npcNo;
		desire = new npcDesire(npcNo, this);
		currentPosition = new Vector3(startingX * 2, WorldManager.manageWorld.heightMap[startingX, startingY], startingY * 2);
	}

	public void setFollowing(Transform newFollowing)
	{
		desire.following = newFollowing;
	}

	public uint getFollowingId()
	{
		if ((bool)desire.following)
		{
			return desire.following.GetComponentInParent<CharMovement>().netId;
		}
		return 0u;
	}

	public bool isAtWork()
	{
		if (desire.lastWalkedInto == null)
		{
			return false;
		}
		if (desire.lastWalkedInto.myLocation == NPCSchedual.Locations.Market_place)
		{
			return true;
		}
		return NPCManager.manage.NPCDetails[npcId].workLocation == desire.lastWalkedInto.myLocation;
	}

	public void saveNpcToMap(Vector3 currentPos)
	{
		currentPosition = currentPos;
		activeNPC = null;
	}

	public void removeSelf()
	{
		if ((bool)activeNPC)
		{
			NPCManager.manage.giveBackNpcDontSave(activeNPC);
		}
	}

	public void pullNpcFromMap(NPCAI myNPC)
	{
		activeNPC = myNPC;
	}

	private void getNewTarget()
	{
		desire.checkDesire(activeNPC);
		if (desire.desiredLocation != 0 || (bool)desire.following)
		{
			currentlyMovingTo = desire.desiredPos;
		}
		else
		{
			currentlyMovingTo = currentPosition;
		}
	}

	public bool hasDesiredRotation()
	{
		if ((bool)desire.wantToWalkInto && (bool)desire.wantToWalkInto.workPos && (bool)activeNPC && activeNPC.isAtWork() && desire.lastWalkedInto == TownManager.manage.allShopFloors[(int)desire.desiredLocation])
		{
			return true;
		}
		return false;
	}

	public Quaternion getDesiredRotation()
	{
		if ((bool)desire.wantToWalkInto && (bool)desire.wantToWalkInto.workPos && (bool)activeNPC && activeNPC.isAtWork())
		{
			return desire.wantToWalkInto.workPos.rotation;
		}
		return activeNPC.transform.rotation;
	}

	public void moveOffNavMesh(Vector3 positionToMoveTo)
	{
		activeNPC.myAgent.transform.position = positionToMoveTo;
		activeNPC.transform.position = positionToMoveTo;
		NetworkNavMesh.nav.UnSpawnNPCOnTile(activeNPC);
	}

	public Vector3 getPositionForLiveAgent()
	{
		if ((bool)desire.following && !activeNPC)
		{
			return desire.following.position;
		}
		if (desire.desiredLocation == NPCSchedual.Locations.Exit || ((bool)desire.lastWalkedInto && desire.lastWalkedInto != TownManager.manage.allShopFloors[(int)desire.desiredLocation]))
		{
			if ((bool)desire.lastWalkedInto)
			{
				return desire.lastWalkedInto.inside.position;
			}
			return Vector3.zero;
		}
		if (desire.desiredLocation == NPCSchedual.Locations.Wonder)
		{
			desire.wantToWalkInto = null;
			if (Random.Range(0, 55) <= 3)
			{
				float num = Random.Range(30f, 80f);
				if (NPCManager.manage.npcStatus[npcId].checkIfHasMovedIn() && (bool)TownManager.manage.allShopFloors[(int)NPCManager.manage.NPCDetails[npcId].mySchedual.dailySchedual[6]])
				{
					if (Vector3.Distance(TownManager.manage.allShopFloors[(int)NPCManager.manage.NPCDetails[npcId].mySchedual.dailySchedual[6]].outside.transform.position, currentPosition) > num)
					{
						return TownManager.manage.allShopFloors[(int)NPCManager.manage.NPCDetails[npcId].mySchedual.dailySchedual[6]].outside.transform.position + new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-5f, 5f));
					}
				}
				else if (!NPCManager.manage.npcStatus[npcId].checkIfHasMovedIn() && (bool)TownManager.manage.allShopFloors[(int)NPCManager.manage.visitingSchedual[6]] && Vector3.Distance(TownManager.manage.allShopFloors[(int)NPCManager.manage.visitingSchedual[6]].outside.transform.position, currentPosition) > num)
				{
					return TownManager.manage.allShopFloors[(int)NPCManager.manage.visitingSchedual[6]].outside.transform.position + new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-5f, 5f));
				}
			}
			return Vector3.zero;
		}
		desire.wantToWalkInto = TownManager.manage.allShopFloors[(int)desire.desiredLocation];
		if ((bool)desire.wantToWalkInto)
		{
			if (desire.wantToWalkInto == desire.lastWalkedInto)
			{
				if ((bool)desire.wantToWalkInto.workPos && (bool)activeNPC && activeNPC.isAtWork())
				{
					return desire.wantToWalkInto.workPos.position;
				}
				return Vector3.zero;
			}
			return desire.wantToWalkInto.outside.position;
		}
		return Vector3.zero;
	}

	public void movePosition()
	{
		getNewTarget();
		if (!activeNPC)
		{
			if (NetworkNavMesh.nav.doesPositionHaveNavChunk(Mathf.RoundToInt(currentPosition.x / 2f), Mathf.RoundToInt(currentPosition.z / 2f)))
			{
				Vector3 vector = NPCManager.manage.checkPositionIsOnNavmesh(new Vector3(currentPosition.x, WorldManager.manageWorld.heightMap[Mathf.RoundToInt(currentPosition.x / 2f), Mathf.RoundToInt(currentPosition.z / 2f)], currentPosition.z));
				if (vector != Vector3.zero)
				{
					currentPosition = vector;
					NetworkNavMesh.nav.SpawnAnNPCAtPosition(npcId, currentPosition);
				}
				else
				{
					moveTowardsDesiredPos();
				}
			}
		}
		else
		{
			currentPosition = activeNPC.transform.position;
		}
		if (!activeNPC)
		{
			if (mapMoveTimer > 2f || ((bool)desire.following && mapMoveTimer > 2f))
			{
				mapMoveTimer = 0f;
				moveTowardsDesiredPos();
			}
			else
			{
				mapMoveTimer += 1f;
			}
		}
		if ((bool)debugMarker)
		{
			if ((bool)activeNPC)
			{
				debugMarker.transform.position = activeNPC.transform.position;
			}
			else
			{
				debugMarker.transform.position = currentPosition;
			}
			string[] obj = new string[6] { "Want to walk into: ", null, null, null, null, null };
			NPCBuildingDoors wantToWalkInto = desire.wantToWalkInto;
			obj[1] = (((object)wantToWalkInto != null) ? wantToWalkInto.ToString() : null);
			obj[2] = "\nLast walked into: ";
			NPCBuildingDoors lastWalkedInto = desire.lastWalkedInto;
			obj[3] = (((object)lastWalkedInto != null) ? lastWalkedInto.ToString() : null);
			obj[4] = "\nAt work:";
			obj[5] = isAtWork().ToString();
			string text = string.Concat(obj);
			debugMarker.GetComponentInChildren<TextMeshPro>().text = text;
		}
	}

	private void moveTowardsDesiredPos()
	{
		if (currentlyMovingTo == Vector3.zero || currentlyMovingTo == currentPosition)
		{
			if (Random.Range(0, 20) == 2)
			{
				currentlyMovingTo = currentPosition + new Vector3(Random.Range(-4, 5), 0f, Random.Range(-4, 5));
				currentlyMovingTo = getWalkableTileForMapNpc(currentlyMovingTo, WorldManager.manageWorld.fencedOffMap[(int)currentPosition.x / 2, (int)currentPosition.z / 2]);
				if (!WorldManager.manageWorld.waterMap[(int)currentPosition.x / 2, (int)currentPosition.z / 2] && Mathf.Abs(WorldManager.manageWorld.heightMap[(int)currentPosition.x / 2, (int)currentPosition.z / 2] - WorldManager.manageWorld.heightMap[(int)currentlyMovingTo.x / 2, (int)currentlyMovingTo.z / 2]) <= 1)
				{
					currentPosition = currentlyMovingTo;
				}
			}
			return;
		}
		Vector3 vector = currentPosition;
		if (Vector3.Distance(currentPosition, currentlyMovingTo) > 8f)
		{
			if (Vector3.Distance(currentPosition, currentlyMovingTo) > 16f)
			{
				vector = currentPosition + (currentlyMovingTo - currentPosition).normalized * 8f;
				vector.y = WorldManager.manageWorld.heightMap[Mathf.RoundToInt(vector.x / 2f), Mathf.RoundToInt(vector.z / 2f)];
				vector = getWalkableTileForMapNpc(vector, WorldManager.manageWorld.fencedOffMap[(int)currentPosition.x / 2, (int)currentPosition.z / 2]);
				if (vector != Vector3.zero)
				{
					currentPosition = vector;
				}
			}
			else
			{
				currentPosition = currentlyMovingTo;
			}
		}
		else
		{
			currentPosition = currentlyMovingTo;
		}
	}

	public Vector3 getWalkableTileForMapNpc(Vector3 checkPos, int currentFencePos)
	{
		int num = Mathf.RoundToInt(checkPos.x / 2f);
		int num2 = Mathf.RoundToInt(checkPos.z / 2f);
		int num3 = 5;
		if (isSpaceStandable(num, num2, currentFencePos))
		{
			return checkPos;
		}
		for (int i = -num3; i < num3; i++)
		{
			for (int j = -num3; j < num3; j++)
			{
				if (isSpaceStandable(num + i, num2 + j, currentFencePos))
				{
					return new Vector3((num + i) * 2, WorldManager.manageWorld.heightMap[num, num2], (num2 + j) * 2);
				}
			}
		}
		return Vector3.zero;
	}

	private bool isSpaceStandable(int xPos, int yPos, int currentFencePos)
	{
		if (xPos == 0 && yPos == 0)
		{
			return true;
		}
		if (!WorldManager.manageWorld.isPositionOnMap(xPos, yPos))
		{
			return false;
		}
		if (currentFencePos <= 0 && WorldManager.manageWorld.fencedOffMap[xPos, yPos] > 0)
		{
			return false;
		}
		if ((!WorldManager.manageWorld.waterMap[xPos, yPos] || (WorldManager.manageWorld.waterMap[xPos, yPos] && WorldManager.manageWorld.heightMap[xPos, yPos] >= 0)) && (WorldManager.manageWorld.onTileMap[xPos, yPos] == -1 || (WorldManager.manageWorld.onTileMap[xPos, yPos] >= 0 && WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].walkable)))
		{
			return true;
		}
		return false;
	}

	public void setBuildingCurrentlyIn(NPCSchedual.Locations locationToSet)
	{
		desire.lastWalkedInto = TownManager.manage.allShopFloors[(int)locationToSet];
	}

	public void warpNpcInside()
	{
		if ((bool)activeNPC && (bool)desire.wantToWalkInto)
		{
			desire.warpNpcToDesiredPos(activeNPC);
		}
		else if (!activeNPC && (bool)desire.wantToWalkInto)
		{
			desire.lastWalkedInto = desire.wantToWalkInto;
			NetworkNavMesh.nav.SpawnAnNPCFromMapToPlaceInBuilding(npcId, desire.wantToWalkInto.inside.position);
			desire.wantToWalkInto = null;
		}
	}

	public bool checkIfNPCHasJustExited()
	{
		if (RealWorldTimeLight.time.currentMinute <= 5)
		{
			return desire.desiredLocation == NPCSchedual.Locations.Exit;
		}
		return false;
	}

	public bool checkIfNPCIsFree()
	{
		if ((bool)activeNPC && activeNPC.followingNetId != 0)
		{
			return true;
		}
		if (desire.desiredLocation == NPCSchedual.Locations.Wonder)
		{
			return true;
		}
		if (desire.desiredLocation == NPCSchedual.Locations.Exit && !desire.wantToWalkInto && !desire.lastWalkedInto)
		{
			return true;
		}
		return false;
	}

	public void setNewDayDesire()
	{
		desire.following = null;
		desire.wantToWalkInto = null;
		desire.lastWalkedInto = null;
		desire.desiredLocation = NPCSchedual.Locations.Wonder;
		desire.checkDesire(activeNPC, 6);
		getPositionForLiveAgent();
	}
}
