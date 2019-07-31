using CompteEstBon;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CebControllerCore {
    [Route("[controller]")]
    [Produces("application/json")]
    public class CompteController : ControllerBase {

        private readonly IMemoryCache _cache;

        private CebTirage Tirage => _cache.GetOrCreate(HttpContext.Session.Id, entry => {
            
            HttpContext.Session.Set("ceb", new byte[] { 1 });
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            return new CebTirage();
        });

        public CompteController(IMemoryCache cache) {
            _cache = cache;
        }


        [HttpGet("[action]")]
        public CebResult Init() {
            Tirage.Random();
            return Tirage.GetCebResult();
        }

        [HttpPost("[action]")]
        public CebResult Clear() {
            Tirage.Clear();
            return Tirage.GetCebResult();
        }

        [HttpPost("[action]")]
        public CebResult Resolve() {
            Tirage.Resolve();
            return Tirage.GetCebResult();
        }

        [HttpPost("[action]")]
        public CebResult Random() {
            Tirage.Random();
            return Tirage.GetCebResult();
        }

        [HttpGet("[action]")]
        public IEnumerable<IEnumerable<string>> GetSolutions() {
            return Tirage.OperationsSolutions;
        }

        [HttpGet("[action]")]
        public IEnumerable<CebDetail> GetSolutionsDetail() {
            return Tirage.ToCebDetails();
        }

        [HttpGet("[action]")]
        public IEnumerable<int> ListePlaques() {
            return CebPlaque.ListePlaques.Distinct();
        }

        [HttpGet("[action]")]
        public IEnumerable<int> GetPlaques() {
            return Tirage.Plaques.Select(p => p.Value);
        }

        [HttpPut("[action]")]
        public CebStatus SetPlaques([FromBody] IList<int> liste) {
            if (liste.Count > 6) return CebStatus.Indefini;
            Tirage.SetPlaques(liste.ToArray());
            return Tirage.Status;
        }

        [HttpGet("[action]")]
        public int GetPlaque(int p) {
            return p > 5 ? 0 : Tirage.Plaques[p].Value;
        }

        [HttpPut("[action]/{No}")]
        public CebStatus SetPlaque(int No, int Value) {
            if (No > 5) return CebStatus.Indefini;
            var t = Tirage;
            t.Plaques[No].Value = Value;
            return t.Status;
        }

        [HttpGet("[action]")]
        public int GetSearch() {
            return Tirage.Search;
        }

        [HttpPut("[action]")]
        public CebStatus SetSearch([FromQuery] int search) {
            Tirage.Search = search;
            return Tirage.Status;
        }

        [HttpGet("[action]")]
        public string GetSolution([FromQuery] int No) {
            var t = Tirage.Solutions;
            return No >= t.Count ? string.Empty : t[No].ToString();
        }

        [HttpPost("[action]")]
        public void Reset() {
            _cache.Remove(HttpContext.Session.Id);
            HttpContext.Session.Remove("ceb");
        }
        [HttpGet("[action]")]
        public string GetVersion() => $"{Environment.Version}";

    }
}
