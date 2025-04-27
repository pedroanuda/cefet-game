using Godot;
using System;

/* Foi necessario colocar o atributo Tool porque um erro estava acontecendo ao atribuirlo
 * no NPC. 
 */

namespace Game.Entities
{
    [GlobalClass, Icon("res://misc/engine_ui/character_icon.svg"), Tool]
    public partial class Character : Resource
    {
        [Export]
        public SpriteFrames SpriteAnimations { get; set; }

        [Export]
        public Vector2 SpritesOffset { get; set; }

        [Export]
        public string Name { get; set; }
    }
}
