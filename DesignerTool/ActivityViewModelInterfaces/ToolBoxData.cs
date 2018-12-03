//-----------------------------------------------------------------------

// <copyright file="ToolBoxData.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ActivityViewModelInterfaces
{
    public class ToolBoxData
    {

        public string ImageUrl { get; private set; }
        public Type Type { get; private set; }
        public ImageSource Image { get; private set; }
        public string ActivityName { get; set; }
        public ToolBoxData(string imageUrl, string activityName, Type type)
        {
            this.ImageUrl = imageUrl;
            this.Type = type;
            this.ActivityName = activityName;

            //var path = System.IO.Path.GetFullPath(imageUrl);
            //Uri imagePath = new Uri(path, UriKind.Absolute);
            //ImageSource source = null;
            //if (!System.IO.File.Exists(path))
            //{
            //    source = new BitmapImage(new Uri(System.IO.Path.GetFullPath($"{__imagesFolder}{__missing_Image_Filename}"), UriKind.Absolute));
            //}
            //else
            //{
            //    source = new BitmapImage(imagePath);
            //}
            Image = ActivityIconGetter.GetOrDefault(ImageUrl);
        }
    }
}
