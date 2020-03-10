using System;
using HamPig.Network;
namespace AuctionWar
{
	public enum ProtocType
	{
		CancelCombatMatch,
		CombatMatch,
		CombatMatchRes,
		CombatResult,
		Heartbeat,
		Login,
		LoginRes,
		QuitCombat,
		ServerOverload,
	}
	public sealed class CancelCombatMatchListener : NetManager.BaseListener<CancelCombatMatchListener, CancelCombatMatch>
	{
		public override short GetProtocType() => (Int16)ProtocType.CancelCombatMatch;
		protected override CancelCombatMatch ParseData(byte[] data, int offset, int size) => CancelCombatMatch.Parser.ParseFrom(data, offset, size);
	}
	public sealed class CombatMatchListener : NetManager.BaseListener<CombatMatchListener, CombatMatch>
	{
		public override short GetProtocType() => (Int16)ProtocType.CombatMatch;
		protected override CombatMatch ParseData(byte[] data, int offset, int size) => CombatMatch.Parser.ParseFrom(data, offset, size);
	}
	public sealed class CombatMatchResListener : NetManager.BaseListener<CombatMatchResListener, CombatMatchRes>
	{
		public override short GetProtocType() => (Int16)ProtocType.CombatMatchRes;
		protected override CombatMatchRes ParseData(byte[] data, int offset, int size) => CombatMatchRes.Parser.ParseFrom(data, offset, size);
	}
	public sealed class CombatResultListener : NetManager.BaseListener<CombatResultListener, CombatResult>
	{
		public override short GetProtocType() => (Int16)ProtocType.CombatResult;
		protected override CombatResult ParseData(byte[] data, int offset, int size) => CombatResult.Parser.ParseFrom(data, offset, size);
	}
	public sealed class HeartbeatListener : NetManager.BaseListener<HeartbeatListener, Heartbeat>
	{
		public override short GetProtocType() => (Int16)ProtocType.Heartbeat;
		protected override Heartbeat ParseData(byte[] data, int offset, int size) => Heartbeat.Parser.ParseFrom(data, offset, size);
	}
	public sealed class LoginListener : NetManager.BaseListener<LoginListener, Login>
	{
		public override short GetProtocType() => (Int16)ProtocType.Login;
		protected override Login ParseData(byte[] data, int offset, int size) => Login.Parser.ParseFrom(data, offset, size);
	}
	public sealed class LoginResListener : NetManager.BaseListener<LoginResListener, LoginRes>
	{
		public override short GetProtocType() => (Int16)ProtocType.LoginRes;
		protected override LoginRes ParseData(byte[] data, int offset, int size) => LoginRes.Parser.ParseFrom(data, offset, size);
	}
	public sealed class QuitCombatListener : NetManager.BaseListener<QuitCombatListener, QuitCombat>
	{
		public override short GetProtocType() => (Int16)ProtocType.QuitCombat;
		protected override QuitCombat ParseData(byte[] data, int offset, int size) => QuitCombat.Parser.ParseFrom(data, offset, size);
	}
	public sealed class ServerOverloadListener : NetManager.BaseListener<ServerOverloadListener, ServerOverload>
	{
		public override short GetProtocType() => (Int16)ProtocType.ServerOverload;
		protected override ServerOverload ParseData(byte[] data, int offset, int size) => ServerOverload.Parser.ParseFrom(data, offset, size);
	}
}
