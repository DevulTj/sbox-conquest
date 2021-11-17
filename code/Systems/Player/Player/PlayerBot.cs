using Sandbox;
using System.Threading.Tasks;

namespace Conquest;

public class PlayerBot : Bot
{
	protected static async Task WaitToMove( Client cl, Vector3 worldPos, Angles ang )
	{
		await GameTask.NextPhysicsFrame();

		cl.Pawn.Position = worldPos;
		cl.Pawn.Rotation = Rotation.From( 0.0f, ang.yaw, 0.0f );
		cl.Pawn.EyeRot = cl.Pawn.Rotation;
	}

	[AdminCmd("conquest_ai_add", Help = "Spawn a Conquest bot.")]
	internal static void SpawnCustomBot()
	{
		Host.AssertServer();

		var caller = ConsoleSystem.Caller.Pawn;

		// Create an instance of your custom bot.
		var bot = new PlayerBot();

		var tr = Trace.Ray( caller.EyePos, caller.EyePos + caller.EyeRot.Forward * 10000f )
			.Radius( 8 )
			.Ignore( caller )
			.Run();

		var callerAng = caller.EyeRot.Angles();

		_ = WaitToMove( bot.Client, tr.EndPos, callerAng.WithYaw( callerAng.yaw + 180f ) );
	}

	public override void BuildInput(InputBuilder builder)
	{
		// Here we can choose / modify the bot's input each tick.
		// We'll make them constantly attack by holding down the Attack1 button.
		builder.SetButton(InputButton.Attack1, true);
	}

	public override void Tick()
	{
	}
}
