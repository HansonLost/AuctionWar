using System;
using HamPig.Network;
namespace AuctionWar
{
	public enum ProtocType
	{
		CombatMatch,
		CombatMatchRes,
		Heartbeat,
		Login,
		LoginRes,
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
}
