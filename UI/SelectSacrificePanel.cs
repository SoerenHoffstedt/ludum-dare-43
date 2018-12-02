using Barely.Util;
using BarelyUI;
using Glide;
using LD43.Scenes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43.UI
{
    public class SelectSacrificePanel : VerticalLayout
    {
        
        Point openPos;
        Point closePos;
        float openCloseLength = 0.33f;
        Text text;        

        public SelectSacrificePanel(Point pos, Point size) 
            : base(pos, size)
        {
            
            text = new Text(Texts.Get("selectSacrificeText"));
            AddChild(text);
            Close();            
            openPos = pos;
            closePos = new Point(openPos.X, 0 - size.Y);
            Position = closePos;
            X = closePos.X;
            Y = closePos.Y;
            OnOpen = () => { GameScene.UITweener.Tween(this, new { X = openPos.X, Y = openPos.Y }, openCloseLength).Round().Ease(Ease.QuadOut); };            
        }

        public void ChangeTextToEnd()
        {
            text.SetText("youLost");
        }

        public void MyClose()
        {
            GameScene.UITweener.Tween(this, new { X = closePos.X, Y = closePos.Y }, openCloseLength).Round().OnComplete(() => this.Close()).Ease(Ease.QuadIn);
        }

    }
}
