//-----------------------------------------------------------------------

// <copyright file="ConditionCollectionFactory.cs" company="Breanos GmbH">
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
using System.Text;

namespace CyPAN.RuntimeAssemblyBuilder
{
    /// <summary>
    /// factory implementation for CWF's transition condition method classes
    /// </summary>
    public class ConditionCollectionFactory
    {
        /// <summary>
        /// the internally used ConditionCollectionClassDefinition
        /// </summary>
        private ConditionCollectionClassDefinition _definition;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="def"></param>
        public ConditionCollectionFactory(ConditionCollectionClassDefinition def)
        {
            _definition = def;

        }
        /// <summary>
        /// Adds/Replaces a boolean method of given name for checking against a value of the data type to be used 
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="condition"></param>
        public void AddConditionMethod(string methodName, string condition)
        {
            condition = condition.Replace(";", "");//sanitize
            _definition.SetMethod(true, true, "System.Boolean", methodName, $"    return ({condition});", (_definition.DataTypeFullName, "x"));
//            _definition.Methods[methodName] =
//                $@"public static bool {methodName}({_definition.DataTypeFullName} x)
//{{
//    return ({condition});
//}}";
        }
        /// <summary>
        /// Finishes creation of the ConditionCollectionClassDefinition and returns it. 
        /// This object of type ConditionCollectionFactory looses its function after calling Finish()
        /// </summary>
        /// <returns></returns>
        public ConditionCollectionClassDefinition Finish()
        {
            var buffer = _definition;
            _definition = null;
            return buffer;
        }
    }
}
