using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TanksRebirth.Internals.Common.Utilities;
using FontStashSharp;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.Tanks.AI;
using TanksRebirth.GameContent.Tanks;

// you'd really need to setup your namespaces properly.
// CobaltsArmada.Script.UI
namespace CobaltsArmada;

/// <summary>
/// Cause we need one
/// </summary>

// Parameters and variables are always camelCase
// Methods, classes, and properties are all PascalCase
// public + static fields are typically PascalCase
// const fields are either PascalCase or SCREAMING_SNAKE_CASE
// public instance fields can be either camelCase or PascalCase
// 
public class VindicationTimer(AITank owner, float startTime = 140)
{
    float _isRising;
    float _animUp;

    public AITank? Owner = owner;

    public float TimeLeft = startTime * 60f;
    public float BossHPMax;

    public string TimerText = string.Empty;

    public void Update()
    {
        if (Owner is null) return;
        if (_animUp == 1f) TimeLeft -= RuntimeData.DeltaTime;
        // use conditional to save a number of cpu cycles
        if (TimeLeft < 0f) TimeLeft = 0f;

        float realTime = TimeLeft / 60f;

        int minutes = (int)(realTime / 60f);
        int seconds = (int)(realTime % 60f);

        // string formatting! it's magical
        // CS3 response: this code is old as dirt. 
        TimerText = $"{minutes:D2}:{seconds:D2}";

        if (realTime > 0f) return;

        var tanks = GameHandler.AllTanks;
        for (int i = 0; i < tanks.Length; i++)
        {
            if (tanks[i] is null || tanks[i] is not PlayerTank plr) continue;
            if (plr is null || plr.IsDestroyed) continue;

            //Shell.Create(plr.Position, Vector2.Zero, 0, Owner);
        }
    }
    // Inverse Lerp is in MathUtils. It's not necessary here

    // completely unused parameters: Vector2 scale, Anchor aligning, Color emptyColor, Color fillColor
    public void Render(SpriteBatch sb, Vector2 position)
    {
        float realTime = TimeLeft / 60f;

        if (CampaignGlobals.InMission)
        {
            _isRising += RuntimeData.DeltaTime / 60f;
            _isRising = MathHelper.Clamp(_isRising, 0f, 1f);
        }
        _animUp += (CampaignGlobals.InMission && Owner is not null && !Owner.IsDestroyed ? 0.025f : -0.025f) * RuntimeData.DeltaTime;
        _animUp = MathHelper.Clamp(_animUp, 0f, 1f);

        var finalPos = position + Vector2.UnitY * 120f * Easings.InBack(1f - _animUp);

        // You typically want to split weirdly complicated things into their own readable variables
        // comments can help too, but variables are better because you can physically see what the code is doing
        // instead of doing a ballpark guess.

        // fractional second
        float timeFraction = realTime % 1f;

        // pulse amount
        float pulse = MathUtils.InverseLerp(0.7f, 1f, timeFraction);

        sb.DrawString(FontGlobals.RebirthFont, TimerText,
            finalPos + (Vector2.UnitY * 20).ToResolution(),

            Color.Lerp(Color.White, Color.Red, pulse),
            new Vector2(2f + (0.5f * pulse)).ToResolution(),
            0f,

            FontGlobals.RebirthFont.MeasureString(TimerText) / 2f, 0f);

        //sb.Draw(TextureGlobals.Pixels[Color.White], finalPos, null, emptyColor, 0f, GameUtils.GetAnchor(aligning, TextureGlobals.Pixels[Color.White].Size()), new Vector2(scale.X, scale.Y), default, 0f);
        //sb.Draw(TextureGlobals.Pixels[Color.White], finalPos - new Vector2((scale.X / 2f) - (scale.X / 2f) * (Hp_Ticked / BossHPMax * Easings.InExpo(_isRising)) ,0), null, Color.White, 0f, GameUtils.GetAnchor(aligning, TextureGlobals.Pixels[Color.White].Size()), new Vector2(scale.X * (Hp_Ticked / BossHPMax * Easings.InExpo(_isRising)), scale.Y), default, 0f);
        //sb.Draw(TextureGlobals.Pixels[Color.White], finalPos - new Vector2((scale.X / 2f) - (scale.X / 2f) * (BossHP / BossHPMax * Easings.InExpo(_isRising)), 0), null, fillColor, 0f, GameUtils.GetAnchor(aligning, TextureGlobals.Pixels[Color.White].Size()), new Vector2(scale.X * (BossHP / BossHPMax * Easings.InExpo(_isRising)), scale.Y), default, 0f);
    }
}