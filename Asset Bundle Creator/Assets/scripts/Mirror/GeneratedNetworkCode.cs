using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Mirror
{
	[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
	public static class GeneratedNetworkCode
	{
		public static ReadyMessage _Read_Mirror_002EReadyMessage(NetworkReader reader)
		{
			return default(ReadyMessage);
		}

		public static void _Write_Mirror_002EReadyMessage(NetworkWriter writer, ReadyMessage value)
		{
		}

		public static NotReadyMessage _Read_Mirror_002ENotReadyMessage(NetworkReader reader)
		{
			return default(NotReadyMessage);
		}

		public static void _Write_Mirror_002ENotReadyMessage(NetworkWriter writer, NotReadyMessage value)
		{
		}

		public static AddPlayerMessage _Read_Mirror_002EAddPlayerMessage(NetworkReader reader)
		{
			return default(AddPlayerMessage);
		}

		public static void _Write_Mirror_002EAddPlayerMessage(NetworkWriter writer, AddPlayerMessage value)
		{
		}

		public static SceneMessage _Read_Mirror_002ESceneMessage(NetworkReader reader)
		{
			SceneMessage result = default(SceneMessage);
			result.sceneName = reader.ReadString();
			result.sceneOperation = _Read_Mirror_002ESceneOperation(reader);
			result.customHandling = reader.ReadBool();
			return result;
		}

		public static SceneOperation _Read_Mirror_002ESceneOperation(NetworkReader reader)
		{
			return (SceneOperation)NetworkReaderExtensions.ReadByte(reader);
		}

		public static void _Write_Mirror_002ESceneMessage(NetworkWriter writer, SceneMessage value)
		{
			writer.WriteString(value.sceneName);
			_Write_Mirror_002ESceneOperation(writer, value.sceneOperation);
			writer.WriteBool(value.customHandling);
		}

		public static void _Write_Mirror_002ESceneOperation(NetworkWriter writer, SceneOperation value)
		{
			NetworkWriterExtensions.WriteByte(writer, (byte)value);
		}

		public static CommandMessage _Read_Mirror_002ECommandMessage(NetworkReader reader)
		{
			CommandMessage result = default(CommandMessage);
			result.netId = reader.ReadUInt();
			result.componentIndex = reader.ReadInt();
			result.functionHash = reader.ReadInt();
			result.payload = reader.ReadBytesAndSizeSegment();
			return result;
		}

		public static void _Write_Mirror_002ECommandMessage(NetworkWriter writer, CommandMessage value)
		{
			writer.WriteUInt(value.netId);
			writer.WriteInt(value.componentIndex);
			writer.WriteInt(value.functionHash);
			writer.WriteBytesAndSizeSegment(value.payload);
		}

		public static RpcMessage _Read_Mirror_002ERpcMessage(NetworkReader reader)
		{
			RpcMessage result = default(RpcMessage);
			result.netId = reader.ReadUInt();
			result.componentIndex = reader.ReadInt();
			result.functionHash = reader.ReadInt();
			result.payload = reader.ReadBytesAndSizeSegment();
			return result;
		}

		public static void _Write_Mirror_002ERpcMessage(NetworkWriter writer, RpcMessage value)
		{
			writer.WriteUInt(value.netId);
			writer.WriteInt(value.componentIndex);
			writer.WriteInt(value.functionHash);
			writer.WriteBytesAndSizeSegment(value.payload);
		}

		public static SpawnMessage _Read_Mirror_002ESpawnMessage(NetworkReader reader)
		{
			SpawnMessage result = default(SpawnMessage);
			result.netId = reader.ReadUInt();
			result.isLocalPlayer = reader.ReadBool();
			result.isOwner = reader.ReadBool();
			result.sceneId = reader.ReadULong();
			result.assetId = reader.ReadGuid();
			result.position = reader.ReadVector3();
			result.rotation = reader.ReadQuaternion();
			result.scale = reader.ReadVector3();
			result.payload = reader.ReadBytesAndSizeSegment();
			return result;
		}

		public static void _Write_Mirror_002ESpawnMessage(NetworkWriter writer, SpawnMessage value)
		{
			writer.WriteUInt(value.netId);
			writer.WriteBool(value.isLocalPlayer);
			writer.WriteBool(value.isOwner);
			writer.WriteULong(value.sceneId);
			writer.WriteGuid(value.assetId);
			writer.WriteVector3(value.position);
			writer.WriteQuaternion(value.rotation);
			writer.WriteVector3(value.scale);
			writer.WriteBytesAndSizeSegment(value.payload);
		}

		public static ObjectSpawnStartedMessage _Read_Mirror_002EObjectSpawnStartedMessage(NetworkReader reader)
		{
			return default(ObjectSpawnStartedMessage);
		}

		public static void _Write_Mirror_002EObjectSpawnStartedMessage(NetworkWriter writer, ObjectSpawnStartedMessage value)
		{
		}

		public static ObjectSpawnFinishedMessage _Read_Mirror_002EObjectSpawnFinishedMessage(NetworkReader reader)
		{
			return default(ObjectSpawnFinishedMessage);
		}

		public static void _Write_Mirror_002EObjectSpawnFinishedMessage(NetworkWriter writer, ObjectSpawnFinishedMessage value)
		{
		}

		public static ObjectDestroyMessage _Read_Mirror_002EObjectDestroyMessage(NetworkReader reader)
		{
			ObjectDestroyMessage result = default(ObjectDestroyMessage);
			result.netId = reader.ReadUInt();
			return result;
		}

		public static void _Write_Mirror_002EObjectDestroyMessage(NetworkWriter writer, ObjectDestroyMessage value)
		{
			writer.WriteUInt(value.netId);
		}

		public static ObjectHideMessage _Read_Mirror_002EObjectHideMessage(NetworkReader reader)
		{
			ObjectHideMessage result = default(ObjectHideMessage);
			result.netId = reader.ReadUInt();
			return result;
		}

		public static void _Write_Mirror_002EObjectHideMessage(NetworkWriter writer, ObjectHideMessage value)
		{
			writer.WriteUInt(value.netId);
		}

		public static EntityStateMessage _Read_Mirror_002EEntityStateMessage(NetworkReader reader)
		{
			EntityStateMessage result = default(EntityStateMessage);
			result.netId = reader.ReadUInt();
			result.payload = reader.ReadBytesAndSizeSegment();
			return result;
		}

		public static void _Write_Mirror_002EEntityStateMessage(NetworkWriter writer, EntityStateMessage value)
		{
			writer.WriteUInt(value.netId);
			writer.WriteBytesAndSizeSegment(value.payload);
		}

		public static NetworkPingMessage _Read_Mirror_002ENetworkPingMessage(NetworkReader reader)
		{
			NetworkPingMessage result = default(NetworkPingMessage);
			result.clientTime = reader.ReadDouble();
			return result;
		}

		public static void _Write_Mirror_002ENetworkPingMessage(NetworkWriter writer, NetworkPingMessage value)
		{
			writer.WriteDouble(value.clientTime);
		}

		public static NetworkPongMessage _Read_Mirror_002ENetworkPongMessage(NetworkReader reader)
		{
			NetworkPongMessage result = default(NetworkPongMessage);
			result.clientTime = reader.ReadDouble();
			result.serverTime = reader.ReadDouble();
			return result;
		}

		public static void _Write_Mirror_002ENetworkPongMessage(NetworkWriter writer, NetworkPongMessage value)
		{
			writer.WriteDouble(value.clientTime);
			writer.WriteDouble(value.serverTime);
		}

		public static void _Write_System_002EInt32_005B_005D(NetworkWriter writer, int[] value)
		{
			writer.WriteArray(value);
		}

		public static int[] _Read_System_002EInt32_005B_005D(NetworkReader reader)
		{
			return reader.ReadArray<int>();
		}

		public static void _Write_System_002EBoolean_005B_005D(NetworkWriter writer, bool[] value)
		{
			writer.WriteArray(value);
		}

		public static bool[] _Read_System_002EBoolean_005B_005D(NetworkReader reader)
		{
			return reader.ReadArray<bool>();
		}

		public static void _Write_ItemOnTop_005B_005D(NetworkWriter writer, ItemOnTop[] value)
		{
			writer.WriteArray(value);
		}

		public static void _Write_ItemOnTop(NetworkWriter writer, ItemOnTop value)
		{
			if (value == null)
			{
				writer.WriteBool(false);
				return;
			}
			writer.WriteBool(true);
			writer.WriteInt(value.sittingOnX);
			writer.WriteInt(value.sittingOnY);
			writer.WriteInt(value.houseX);
			writer.WriteInt(value.houseY);
			writer.WriteInt(value.onTopPosition);
			writer.WriteInt(value.itemId);
			writer.WriteInt(value.itemStatus);
			writer.WriteInt(value.itemRotation);
		}

		public static ItemOnTop[] _Read_ItemOnTop_005B_005D(NetworkReader reader)
		{
			return reader.ReadArray<ItemOnTop>();
		}

		public static ItemOnTop _Read_ItemOnTop(NetworkReader reader)
		{
			if (!reader.ReadBool())
			{
				return null;
			}
			ItemOnTop itemOnTop = new ItemOnTop();
			itemOnTop.sittingOnX = reader.ReadInt();
			itemOnTop.sittingOnY = reader.ReadInt();
			itemOnTop.houseX = reader.ReadInt();
			itemOnTop.houseY = reader.ReadInt();
			itemOnTop.onTopPosition = reader.ReadInt();
			itemOnTop.itemId = reader.ReadInt();
			itemOnTop.itemStatus = reader.ReadInt();
			itemOnTop.itemRotation = reader.ReadInt();
			return itemOnTop;
		}

		public static void _Write_PostOnBoard(NetworkWriter writer, PostOnBoard value)
		{
			if (value == null)
			{
				writer.WriteBool(false);
				return;
			}
			writer.WriteBool(true);
			writer.WriteInt(value.postTextId);
			writer.WriteInt(value.daysToComplete);
			writer.WriteInt(value.postTypeId);
			writer.WriteInt(value.templateType);
			writer.WriteInt(value.postedByNpcId);
			writer.WriteBool(value.hasBeenRead);
			writer.WriteBool(value.accepted);
			writer.WriteBool(value.readyForNPC);
			writer.WriteBool(value.completed);
			writer.WriteInt(value.rewardId);
			writer.WriteInt(value.rewardAmount);
			writer.WriteBool(value.isHuntingTask);
			_Write_HuntingChallenge(writer, value.myHuntingChallenge);
			writer.WriteBool(value.isPhotoTask);
			_Write_PhotoChallenge(writer, value.myPhotoChallenge);
			writer.WriteBool(value.isTrade);
			writer.WriteBool(value.isCaptureTask);
			writer.WriteInt(value.animalToCapture);
			writer.WriteInt(value.captureVariation);
			writer.WriteInt(value.requiredItem);
			writer.WriteInt(value.requireItemAmount);
			writer.WriteBool(value.isInvestigation);
			_Write_System_002EInt32_005B_005D(writer, value.location);
		}

		public static void _Write_HuntingChallenge(NetworkWriter writer, HuntingChallenge value)
		{
			if (value == null)
			{
				writer.WriteBool(false);
				return;
			}
			writer.WriteBool(true);
			_Write_System_002EInt32_005B_005D(writer, value.spawnPos);
			writer.WriteInt(value.challengeAnimalId);
			writer.WriteInt(value.spawnChunkX);
			writer.WriteInt(value.spawnChunkY);
		}

		public static void _Write_PhotoChallenge(NetworkWriter writer, PhotoChallenge value)
		{
			if (value == null)
			{
				writer.WriteBool(false);
				return;
			}
			writer.WriteBool(true);
			_Write_System_002ESingle_005B_005D(writer, value.photoLocation);
			_Write_System_002EInt32_005B_005D(writer, value.photoTime);
			writer.WriteInt(value.subjectType);
			_Write_System_002EInt32_005B_005D(writer, value.subjectIds);
			writer.WriteInt(value.requiredAmountOfSubject);
			writer.WriteInt(value.reward);
		}

		public static void _Write_System_002ESingle_005B_005D(NetworkWriter writer, float[] value)
		{
			writer.WriteArray(value);
		}

		public static PostOnBoard _Read_PostOnBoard(NetworkReader reader)
		{
			if (!reader.ReadBool())
			{
				return null;
			}
			PostOnBoard postOnBoard = new PostOnBoard();
			postOnBoard.postTextId = reader.ReadInt();
			postOnBoard.daysToComplete = reader.ReadInt();
			postOnBoard.postTypeId = reader.ReadInt();
			postOnBoard.templateType = reader.ReadInt();
			postOnBoard.postedByNpcId = reader.ReadInt();
			postOnBoard.hasBeenRead = reader.ReadBool();
			postOnBoard.accepted = reader.ReadBool();
			postOnBoard.readyForNPC = reader.ReadBool();
			postOnBoard.completed = reader.ReadBool();
			postOnBoard.rewardId = reader.ReadInt();
			postOnBoard.rewardAmount = reader.ReadInt();
			postOnBoard.isHuntingTask = reader.ReadBool();
			postOnBoard.myHuntingChallenge = _Read_HuntingChallenge(reader);
			postOnBoard.isPhotoTask = reader.ReadBool();
			postOnBoard.myPhotoChallenge = _Read_PhotoChallenge(reader);
			postOnBoard.isTrade = reader.ReadBool();
			postOnBoard.isCaptureTask = reader.ReadBool();
			postOnBoard.animalToCapture = reader.ReadInt();
			postOnBoard.captureVariation = reader.ReadInt();
			postOnBoard.requiredItem = reader.ReadInt();
			postOnBoard.requireItemAmount = reader.ReadInt();
			postOnBoard.isInvestigation = reader.ReadBool();
			postOnBoard.location = _Read_System_002EInt32_005B_005D(reader);
			return postOnBoard;
		}

		public static HuntingChallenge _Read_HuntingChallenge(NetworkReader reader)
		{
			if (!reader.ReadBool())
			{
				return null;
			}
			HuntingChallenge huntingChallenge = new HuntingChallenge();
			huntingChallenge.spawnPos = _Read_System_002EInt32_005B_005D(reader);
			huntingChallenge.challengeAnimalId = reader.ReadInt();
			huntingChallenge.spawnChunkX = reader.ReadInt();
			huntingChallenge.spawnChunkY = reader.ReadInt();
			return huntingChallenge;
		}

		public static PhotoChallenge _Read_PhotoChallenge(NetworkReader reader)
		{
			if (!reader.ReadBool())
			{
				return null;
			}
			PhotoChallenge photoChallenge = new PhotoChallenge();
			photoChallenge.photoLocation = _Read_System_002ESingle_005B_005D(reader);
			photoChallenge.photoTime = _Read_System_002EInt32_005B_005D(reader);
			photoChallenge.subjectType = reader.ReadInt();
			photoChallenge.subjectIds = _Read_System_002EInt32_005B_005D(reader);
			photoChallenge.requiredAmountOfSubject = reader.ReadInt();
			photoChallenge.reward = reader.ReadInt();
			return photoChallenge;
		}

		public static float[] _Read_System_002ESingle_005B_005D(NetworkReader reader)
		{
			return reader.ReadArray<float>();
		}

		public static void _Write_PostOnBoard_005B_005D(NetworkWriter writer, PostOnBoard[] value)
		{
			writer.WriteArray(value);
		}

		public static PostOnBoard[] _Read_PostOnBoard_005B_005D(NetworkReader reader)
		{
			return reader.ReadArray<PostOnBoard>();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void InitReadWriters()
		{
			Writer<byte>.write = NetworkWriterExtensions.WriteByte;
			Writer<sbyte>.write = NetworkWriterExtensions.WriteSByte;
			Writer<char>.write = NetworkWriterExtensions.WriteChar;
			Writer<bool>.write = NetworkWriterExtensions.WriteBool;
			Writer<ushort>.write = NetworkWriterExtensions.WriteUShort;
			Writer<short>.write = NetworkWriterExtensions.WriteShort;
			Writer<uint>.write = NetworkWriterExtensions.WriteUInt;
			Writer<int>.write = NetworkWriterExtensions.WriteInt;
			Writer<ulong>.write = NetworkWriterExtensions.WriteULong;
			Writer<long>.write = NetworkWriterExtensions.WriteLong;
			Writer<float>.write = NetworkWriterExtensions.WriteFloat;
			Writer<double>.write = NetworkWriterExtensions.WriteDouble;
			Writer<decimal>.write = NetworkWriterExtensions.WriteDecimal;
			Writer<string>.write = NetworkWriterExtensions.WriteString;
			Writer<byte[]>.write = NetworkWriterExtensions.WriteBytesAndSize;
			Writer<ArraySegment<byte>>.write = NetworkWriterExtensions.WriteBytesAndSizeSegment;
			Writer<Vector2>.write = NetworkWriterExtensions.WriteVector2;
			Writer<Vector3>.write = NetworkWriterExtensions.WriteVector3;
			Writer<Vector3?>.write = NetworkWriterExtensions.WriteVector3Nullable;
			Writer<Vector4>.write = NetworkWriterExtensions.WriteVector4;
			Writer<Vector2Int>.write = NetworkWriterExtensions.WriteVector2Int;
			Writer<Vector3Int>.write = NetworkWriterExtensions.WriteVector3Int;
			Writer<Color>.write = NetworkWriterExtensions.WriteColor;
			Writer<Color32>.write = NetworkWriterExtensions.WriteColor32;
			Writer<Quaternion>.write = NetworkWriterExtensions.WriteQuaternion;
			Writer<Quaternion?>.write = NetworkWriterExtensions.WriteQuaternionNullable;
			Writer<Rect>.write = NetworkWriterExtensions.WriteRect;
			Writer<Plane>.write = NetworkWriterExtensions.WritePlane;
			Writer<Ray>.write = NetworkWriterExtensions.WriteRay;
			Writer<Matrix4x4>.write = NetworkWriterExtensions.WriteMatrix4x4;
			Writer<Guid>.write = NetworkWriterExtensions.WriteGuid;
			Writer<NetworkIdentity>.write = NetworkWriterExtensions.WriteNetworkIdentity;
			Writer<NetworkBehaviour>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
			Writer<Transform>.write = NetworkWriterExtensions.WriteTransform;
			Writer<GameObject>.write = NetworkWriterExtensions.WriteGameObject;
			Writer<Uri>.write = NetworkWriterExtensions.WriteUri;
			Writer<ReadyMessage>.write = _Write_Mirror_002EReadyMessage;
			Writer<NotReadyMessage>.write = _Write_Mirror_002ENotReadyMessage;
			Writer<AddPlayerMessage>.write = _Write_Mirror_002EAddPlayerMessage;
			Writer<SceneMessage>.write = _Write_Mirror_002ESceneMessage;
			Writer<SceneOperation>.write = _Write_Mirror_002ESceneOperation;
			Writer<CommandMessage>.write = _Write_Mirror_002ECommandMessage;
			Writer<RpcMessage>.write = _Write_Mirror_002ERpcMessage;
			Writer<SpawnMessage>.write = _Write_Mirror_002ESpawnMessage;
			Writer<ObjectSpawnStartedMessage>.write = _Write_Mirror_002EObjectSpawnStartedMessage;
			Writer<ObjectSpawnFinishedMessage>.write = _Write_Mirror_002EObjectSpawnFinishedMessage;
			Writer<ObjectDestroyMessage>.write = _Write_Mirror_002EObjectDestroyMessage;
			Writer<ObjectHideMessage>.write = _Write_Mirror_002EObjectHideMessage;
			Writer<EntityStateMessage>.write = _Write_Mirror_002EEntityStateMessage;
			Writer<NetworkPingMessage>.write = _Write_Mirror_002ENetworkPingMessage;
			Writer<NetworkPongMessage>.write = _Write_Mirror_002ENetworkPongMessage;
			Writer<int[]>.write = _Write_System_002EInt32_005B_005D;
			Writer<bool[]>.write = _Write_System_002EBoolean_005B_005D;
			Writer<ItemOnTop[]>.write = _Write_ItemOnTop_005B_005D;
			Writer<ItemOnTop>.write = _Write_ItemOnTop;
			Writer<PostOnBoard>.write = _Write_PostOnBoard;
			Writer<HuntingChallenge>.write = _Write_HuntingChallenge;
			Writer<PhotoChallenge>.write = _Write_PhotoChallenge;
			Writer<float[]>.write = _Write_System_002ESingle_005B_005D;
			Writer<PostOnBoard[]>.write = _Write_PostOnBoard_005B_005D;
			Reader<byte>.read = NetworkReaderExtensions.ReadByte;
			Reader<sbyte>.read = NetworkReaderExtensions.ReadSByte;
			Reader<char>.read = NetworkReaderExtensions.ReadChar;
			Reader<bool>.read = NetworkReaderExtensions.ReadBool;
			Reader<short>.read = NetworkReaderExtensions.ReadShort;
			Reader<ushort>.read = NetworkReaderExtensions.ReadUShort;
			Reader<int>.read = NetworkReaderExtensions.ReadInt;
			Reader<uint>.read = NetworkReaderExtensions.ReadUInt;
			Reader<long>.read = NetworkReaderExtensions.ReadLong;
			Reader<ulong>.read = NetworkReaderExtensions.ReadULong;
			Reader<float>.read = NetworkReaderExtensions.ReadFloat;
			Reader<double>.read = NetworkReaderExtensions.ReadDouble;
			Reader<decimal>.read = NetworkReaderExtensions.ReadDecimal;
			Reader<string>.read = NetworkReaderExtensions.ReadString;
			Reader<byte[]>.read = NetworkReaderExtensions.ReadBytesAndSize;
			Reader<ArraySegment<byte>>.read = NetworkReaderExtensions.ReadBytesAndSizeSegment;
			Reader<Vector2>.read = NetworkReaderExtensions.ReadVector2;
			Reader<Vector3>.read = NetworkReaderExtensions.ReadVector3;
			Reader<Vector3?>.read = NetworkReaderExtensions.ReadVector3Nullable;
			Reader<Vector4>.read = NetworkReaderExtensions.ReadVector4;
			Reader<Vector2Int>.read = NetworkReaderExtensions.ReadVector2Int;
			Reader<Vector3Int>.read = NetworkReaderExtensions.ReadVector3Int;
			Reader<Color>.read = NetworkReaderExtensions.ReadColor;
			Reader<Color32>.read = NetworkReaderExtensions.ReadColor32;
			Reader<Quaternion>.read = NetworkReaderExtensions.ReadQuaternion;
			Reader<Quaternion?>.read = NetworkReaderExtensions.ReadQuaternionNullable;
			Reader<Rect>.read = NetworkReaderExtensions.ReadRect;
			Reader<Plane>.read = NetworkReaderExtensions.ReadPlane;
			Reader<Ray>.read = NetworkReaderExtensions.ReadRay;
			Reader<Matrix4x4>.read = NetworkReaderExtensions.ReadMatrix4x4;
			Reader<Guid>.read = NetworkReaderExtensions.ReadGuid;
			Reader<Transform>.read = NetworkReaderExtensions.ReadTransform;
			Reader<GameObject>.read = NetworkReaderExtensions.ReadGameObject;
			Reader<NetworkIdentity>.read = NetworkReaderExtensions.ReadNetworkIdentity;
			Reader<NetworkBehaviour>.read = NetworkReaderExtensions.ReadNetworkBehaviour;
			Reader<NetworkBehaviour.NetworkBehaviourSyncVar>.read = NetworkReaderExtensions.ReadNetworkBehaviourSyncVar;
			Reader<Uri>.read = NetworkReaderExtensions.ReadUri;
			Reader<ReadyMessage>.read = _Read_Mirror_002EReadyMessage;
			Reader<NotReadyMessage>.read = _Read_Mirror_002ENotReadyMessage;
			Reader<AddPlayerMessage>.read = _Read_Mirror_002EAddPlayerMessage;
			Reader<SceneMessage>.read = _Read_Mirror_002ESceneMessage;
			Reader<SceneOperation>.read = _Read_Mirror_002ESceneOperation;
			Reader<CommandMessage>.read = _Read_Mirror_002ECommandMessage;
			Reader<RpcMessage>.read = _Read_Mirror_002ERpcMessage;
			Reader<SpawnMessage>.read = _Read_Mirror_002ESpawnMessage;
			Reader<ObjectSpawnStartedMessage>.read = _Read_Mirror_002EObjectSpawnStartedMessage;
			Reader<ObjectSpawnFinishedMessage>.read = _Read_Mirror_002EObjectSpawnFinishedMessage;
			Reader<ObjectDestroyMessage>.read = _Read_Mirror_002EObjectDestroyMessage;
			Reader<ObjectHideMessage>.read = _Read_Mirror_002EObjectHideMessage;
			Reader<EntityStateMessage>.read = _Read_Mirror_002EEntityStateMessage;
			Reader<NetworkPingMessage>.read = _Read_Mirror_002ENetworkPingMessage;
			Reader<NetworkPongMessage>.read = _Read_Mirror_002ENetworkPongMessage;
			Reader<int[]>.read = _Read_System_002EInt32_005B_005D;
			Reader<bool[]>.read = _Read_System_002EBoolean_005B_005D;
			Reader<ItemOnTop[]>.read = _Read_ItemOnTop_005B_005D;
			Reader<ItemOnTop>.read = _Read_ItemOnTop;
			Reader<PostOnBoard>.read = _Read_PostOnBoard;
			Reader<HuntingChallenge>.read = _Read_HuntingChallenge;
			Reader<PhotoChallenge>.read = _Read_PhotoChallenge;
			Reader<float[]>.read = _Read_System_002ESingle_005B_005D;
			Reader<PostOnBoard[]>.read = _Read_PostOnBoard_005B_005D;
		}
	}
}
