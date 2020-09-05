/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dale Roberts. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MouseUnSnag
{
    // SnagScreen keeps track of the physical screens/monitors attached to the system, and adds
    // some members to keep track of screen relative geometry.
    //
    // N.B. (Note Well!) There is a non-static "instance" of SnagScreen for EACH physical
    // screen/monitor in the system. The *static* members are relative to ALL the screens/monitors.

    public class SnagScreen
    {

        /// <summary> The associated <see cref="Screen"/> </summary>
        public Screen Screen { get; } // Points to the entry in Screen.AllScreens[].
        /// <summary> Screen Number </summary>
        public int ScreenNumber { get; }
        /// <summary> Shortcut to screen.Bounds </summary>
        public Rectangle R => Screen.Bounds;
        /// <summary> Effective DPI </summary>
        public int EffectiveDpi { get; }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString() => ScreenNumber.ToString(CultureInfo.InvariantCulture);

        /// <summary> List of Screens that are to the left </summary>
        public List<SnagScreen> ToLeft { get; }
        /// <summary> List of Screens that are to the right </summary>
        public List<SnagScreen> ToRight { get; }
        /// <summary> List of Screens that are above </summary>
        public List<SnagScreen> Above { get; }
        /// <summary> List of Screens that are below </summary>
        public List<SnagScreen> Below { get; }

        /// <summary>
        /// Initialize a new <see cref="SnagScreen"/>
        /// </summary>
        /// <param name="S"><see cref="Screen"/></param>
        /// <param name="ScreenNum">Screen Number</param>
        public SnagScreen(Screen S, int ScreenNum)
        {
            EffectiveDpi = (int)NativeMethods.GetDpi(S, NativeMethods.DpiType.Effective);
            Screen = S;
            ScreenNumber = ScreenNum;
            ToLeft = new List<SnagScreen>();
            ToRight = new List<SnagScreen>();
            Above = new List<SnagScreen>();
            Below = new List<SnagScreen>();
        }


        // If s is immediately adjacent to (shares a border with) us, then add it to the
        // appropriate direction list. If s is not "touching" us, then it will not get added to
        // any list. s can be added to at most one list (hence use of "else if" instead of just
        // a sequence of "if's").
        public void AddDirectionTo(SnagScreen s)
        {
            if ((R.Right == s.R.Left) && GeometryUtil.OverlapY(R, s.R)) ToRight.Add(s);
            else if ((R.Left == s.R.Right) && GeometryUtil.OverlapY(R, s.R)) ToLeft.Add(s);
            else if ((R.Top == s.R.Bottom) && GeometryUtil.OverlapX(R, s.R)) Above.Add(s);
            else if ((R.Bottom == s.R.Top) && GeometryUtil.OverlapX(R, s.R)) Below.Add(s);
        }

    }
}
