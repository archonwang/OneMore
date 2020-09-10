﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using River.OneMoreAddIn.Dialogs;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class CropImageCommand : Command
	{
		private ApplicationManager manager;
		private Page page;
		private XNamespace ns;


		public CropImageCommand() : base()
		{
		}


		public void Execute()
		{
			using (manager = new ApplicationManager())
			{
				page = new Page(manager.CurrentPage(PageInfo.piAll));
				ns = page.Namespace;

				var images = page.Root.Descendants(ns + "Image")?
					.Where(e => e.Attribute("selected")?.Value == "all");

				if (images?.Count() == 1)
				{
					CropImage(images.First());
				}
				else
				{
					UIHelper.ShowError(Resx.CropImage_oneImage);
				}
			}
		}


		private void CropImage(XElement element)
		{
			var data = element.Element(ns + "Data");
			var binhex = Convert.FromBase64String(data.Value);
			using (var stream = new MemoryStream(binhex, 0, binhex.Length))
			{
				using (var image = Image.FromStream(stream))
				{
					using (var dialog = new CropImageDialog(image))
					{
						var result = dialog.ShowDialog(owner);
						if (result == DialogResult.OK)
						{
							var bytes = (byte[])new ImageConverter()
								.ConvertTo(dialog.Image, typeof(byte[]));

							data.Value = Convert.ToBase64String(bytes);

							// calculate scaled factor of image on page
							var size = element.Element(ns + "Size");
							size.ReadAttributeValue("width", out float ax, image.Width);
							size.ReadAttributeValue("height", out float ay, image.Height);
							float scaleX = ax / image.Width;
							float scaleY = ay / image.Height;

							size.SetAttributeValue("width", $"{(int)(dialog.Image.Width * scaleX)}.0");
							size.SetAttributeValue("height", $"{(int)(dialog.Image.Height * scaleY)}.0");
							size.SetAttributeValue("isSetByUser", "true");

							manager.UpdatePageContent(page.Root);
						}
					}
				}
			}
		}
	}
}
