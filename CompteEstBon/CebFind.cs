//-----------------------------------------------------------------------
// <copyright file="CebFind.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Runtime.InteropServices;

namespace CompteEstBon {
    /// <summary>
    ///
    /// </summary>
    /// <param name="Found1"></param>
    /// <param name="Found2"></param>
    [Guid("9CA27D73-CD46-41CE-B666-3F589F98D328")]
    public record CebFind(int? Found1 = null, int? Found2 = null) {
        /// <summary>
        ///
        /// </summary>
        /// <summary>
        ///
        /// </summary>
        public int? Found1 { get; private set; } = Found1;

        /// <summary>
        ///
        /// </summary>
        public int? Found2 { get; private set; } = Found2;

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        public void Add(int? value) {
            if(value != Found1 && value != Found2) {
                if(value == null)
                    Found2 = null;
                else if(value > Found1)
                    Found2 = value;
                else {
                    Found2 = Found1;
                    Found1 = value;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Found2 == null ? Found1.ToString() : $"{Found1} et {Found2}";

        /// <summary>
        ///
        /// </summary>
        public bool IsUnique => Found2 == null;

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        public void Reset(int? value = null) {
            Found1 = value;
            Found2 = null;
        }
    }
}