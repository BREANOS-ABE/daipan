//-----------------------------------------------------------------------

// <copyright file="ActivityIconGetter.cs" company="Breanos GmbH">
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
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ActivityViewModelInterfaces
{
    public static class ActivityIconGetter
    {
        private const string __DEFAULT_NAME = "MISSING.png";
        public static string ImageFolder { get; set; }

        static ActivityIconGetter()
        {
            ImageFolder = "Images/";
        }
        public static ImageSource GetOrDefault(string imageUrl)
        {
            var path = System.IO.Path.GetFullPath(ImageFolder+imageUrl);
            Uri imagePath = new Uri(path, UriKind.Absolute);
            ImageSource source = null;
            if (!System.IO.File.Exists(path))
            {
                source = new BitmapImage(new Uri(System.IO.Path.GetFullPath($"{ImageFolder}{__DEFAULT_NAME}"), UriKind.Absolute));
            }
            else
            {
                source = new BitmapImage(imagePath);
            }
            return source;
        }
    }
}
