﻿//
// ExtendedTitleBarWindowBackend.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2014 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using MonoDevelop.MacInterop;
using MonoMac.AppKit;
using MonoDevelop.Components;
using Mono.TextEditor;
using MonoDevelop.Components.Extensions;
using Xwt.GtkBackend;

namespace MonoDevelop.MacIntegration
{
	class ExtendedTitleBarWindowBackend: Xwt.GtkBackend.WindowBackend, IExtendedTitleBarWindowBackend
	{
		CustomToolbar toolbar;

		class CustomToolbar: Gtk.EventBox
		{
			public CustomToolbar ()
			{
				WidgetFlags |= Gtk.WidgetFlags.AppPaintable;
			}

			public Cairo.ImageSurface Background {
				get;
				set;
			}

			public int TitleBarHeight {
				get;
				set;
			}

			protected override bool OnExposeEvent (Gdk.EventExpose evnt)
			{
				using (var context = Gdk.CairoHelper.Create (evnt.Window)) {
					context.Rectangle (
						evnt.Area.X,
						evnt.Area.Y,
						evnt.Area.Width,
						evnt.Area.Height
					);
					context.Clip ();
					context.LineWidth = 1;
					if (Background != null && Background.Width > 0) {
						for (int x=0; x < Allocation.Width; x += Background.Width) {
							Background.Show (context, x, -TitleBarHeight);
						}
					} else {
						context.Rectangle (0, 0, Allocation.Width, Allocation.Height);
						using (var lg = new Cairo.LinearGradient (0, 0, 0, Allocation.Height)) {
							lg.AddColorStop (0, Style.Light (Gtk.StateType.Normal).ToCairoColor ());
							lg.AddColorStop (1, Style.Mid (Gtk.StateType.Normal).ToCairoColor ());
							context.SetSource (lg);
						}
						context.Fill ();

					}
					context.MoveTo (0, Allocation.Height - 0.5);
					context.RelLineTo (Allocation.Width, 0);
					context.SetSourceColor (MonoDevelop.Ide.Gui.Styles.ToolbarBottomBorderColor);
					context.Stroke ();

					context.MoveTo (0, Allocation.Height - 1.5);
					context.RelLineTo (Allocation.Width, 0);
					context.SetSourceColor (MonoDevelop.Ide.Gui.Styles.ToolbarBottomGlowColor);
					context.Stroke ();

				}
				return base.OnExposeEvent (evnt);
			}
		}

		public ExtendedTitleBarWindowBackend ()
		{
		}

		public override void Initialize ()
		{
			base.Initialize ();

			var resource = "maintoolbarbg.png";

			Window.Realized += delegate {
				NSWindow w = GtkQuartz.GetWindow (Window);
				w.IsOpaque = false;
				NSImage img = MacPlatformService.LoadImage (resource);
				w.BackgroundColor = NSColor.FromPatternImage (img);
				w.StyleMask |= NSWindowStyle.TexturedBackground;
			};

			toolbar = new CustomToolbar ();
			toolbar.Background = MonoDevelop.Components.CairoExtensions.LoadImage (typeof(MacPlatformService).Assembly, resource);
			toolbar.TitleBarHeight = MacPlatformService.GetTitleBarHeight ();
			MainBox.PackStart (toolbar, false, false, 0);
			((Gtk.Box.BoxChild)MainBox [toolbar]).Position = 0;
		}

		public void SetHeaderContent (Xwt.Backends.IWidgetBackend backend)
		{
			if (toolbar.Child != null) {
				WidgetBackend.RemoveChildPlacement (toolbar.Child);
				toolbar.Remove (toolbar.Child);
			}
			if (backend != null) {
				toolbar.Child = WidgetBackend.GetWidgetWithPlacement (backend);
				toolbar.Show ();
			} else {
				toolbar.Hide ();
			}
		}
	}
}

