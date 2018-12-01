using BarelyUI;
using BarelyUI.Layouts;
using BarelyUI.Styles;
using LD43.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43.UI
{
    public class ButtonPanel : HorizontalLayout
    {
        GameScene game;

        public ButtonPanel(GameScene game) : base()
        {
            this.game = game;

            AddChild(new UIElement[] { });            
        }

    }
}
