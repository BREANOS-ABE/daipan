//-----------------------------------------------------------------------

// <copyright file="ClassFactory.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;

namespace CyPAN.RuntimeAssemblyBuilder
{
    /// <summary>
    /// Static factory class for creating different kinds of class definitions
    /// </summary>
    public static class ClassFactory
    {
        /// <summary>
        /// Factory class creator/getter for CWF's transition condition method classes
        /// </summary>
        /// <param name="ns">Namespace</param>
        /// <param name="classname">Class name</param>
        /// <param name="dataType">the data type of the condition check object to be used.</param>
        /// <param name="additionalUsings">optional additional used namespaces</param>
        /// <returns></returns>
        public static ConditionCollectionFactory CreateWorkflowConditionCollectionClassDefinition(string ns, string classname, Type dataType, params string[] additionalUsings)
        {
            var def = new ConditionCollectionClassDefinition()
            {
                Namespace = ns,
                ClassName = classname,
                DataTypeFullName = dataType.FullName
            };
            foreach (var u in additionalUsings) def.AddUsing(u);
            def.AddAssemblyLocation(dataType.Assembly.Location);
            ConditionCollectionFactory factory = new ConditionCollectionFactory(def);
            return factory;
        }
    }
}
