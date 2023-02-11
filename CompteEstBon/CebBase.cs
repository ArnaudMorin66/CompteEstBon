//-----------------------------------------------------------------------
// <copyright file="CebBase.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// Plage Compte est bon
#pragma warning disable CS1591

using System.Text.Json.Serialization;
using arnaud.morin.outils;

namespace CompteEstBon {
    ///
    /// clase de base plaque ou operation
    ///
    [System.Runtime.InteropServices.Guid("F4D942FB-85DF-4391-AE82-9EFE20DDADB0")] 
    public abstract class CebBase {
        /// <inheritdoc/>
        public override string ToString() => string.Join(", ", Operations);
        /// <summary>
        /// 
        /// </summary>
        public List<string> Operations { get; }

        /// <summary>
        /// Valeur de la donn�e
        /// </summary>
        [JsonIgnore]
        public virtual int Value { get; set; } = 0;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public static implicit operator int(CebBase b) => b.Value;
        
        [JsonIgnore]
        public int Rank => Operations.Count;
        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public abstract bool IsValid { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) => (obj is CebBase op && op.Rank == Rank) && Operations.WithIndex().All(e => string.Compare(e.Item1, op.Operations[e.Item2], StringComparison.Ordinal) == 0);
        /// <inheritdoc/>
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode() => base.GetHashCode();
        /// <summary>
        /// 
        /// </summary>
        protected CebBase() => Operations = new List<string>();
       /// <summary>
       /// 
       /// </summary>
        public CebDetail Detail {
            get {
                CebDetail detail = new();
                foreach (var (operation, i) in Operations.WithIndex()) detail[i] = operation;
                return detail;
            }
        }
       /// <summary>
       /// 
       /// </summary>
       /// <param name="b"></param>
        public static implicit operator CebDetail(CebBase b) => b.Detail;
    }
}