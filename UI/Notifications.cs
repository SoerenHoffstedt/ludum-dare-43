using Barely.Util;
using BarelyUI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43.UI
{
    

    public class Notifications : VerticalLayout
    {

        private class Not
        {
            public string text;
            public float timer;

            public Not(string text)
            {
                this.text = text;
                timer = 0f;
            }
        }

        Text onlyText;



        Not[] lines;

        public Notifications()
        {
            lines = new Not[6];

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = new Not("");
            }

            Point size = new Point(300, 300);
            Point pos  = new Point(0, 0);
            SetFixedSize(size);
            SetPosition(pos);
            layoutSizeY = LayoutSize.WrapContent;

            string testStringLong = "Not enough population capacity."; //take a little longer than this string.
            onlyText = new Text(testStringLong + "\nasdasdad\n13123\n123123\nasdasd\n123123", false);
            onlyText.wrapText = true;
            AddChild(onlyText);
        }

        private static float DISPLAY_LENGTH = 4f;

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            foreach(Not n in lines)
            {
                if(n.text != "")
                {
                    n.timer += deltaTime;
                    if(n.timer >= DISPLAY_LENGTH)
                    {
                        n.text = "";
                        n.timer = 0f;
                    }

                }
            }

            StringBuilder sb = new StringBuilder();

            for(int i = 0; i < lines.Length; i++)
            {
                sb.AppendLine(lines[i].text);
            }

            onlyText.SetText(sb.ToString());
        }

        public void AddNotification(string text)
        {
            for (int i = lines.Length; i > 0; i--)
            {
                if(lines[i - 1].text != "")
                {
                    lines[i].text  = lines[i - 1].text;
                    lines[i].timer = lines[i - 1].timer;

                }
            }

            lines[0].text = Texts.Get(text);
            lines[0].timer = 0f;
        }

    }
}
