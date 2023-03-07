using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
namespace SW_APP.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Poslodavac")]
    public class OglasiController : ControllerBase
    {
    public DvlHubContext Context { get; set ;}
    private IConfiguration _config;

        public OglasiController(DvlHubContext context, IConfiguration config)
        {
              Context=context;
              _config=config;
        }

        private Korisnik VratiKorisnika()
        {
            var Identitet = HttpContext.User.Identity as ClaimsIdentity;
            if(Identitet != null)
            {
                var Tvrdnja = Identitet.Claims;
                return new Korisnik{
                    korisnicko_ime = Tvrdnja.FirstOrDefault(p => p.Type == ClaimTypes.GivenName)?.Value,
                    email = Tvrdnja.FirstOrDefault(p => p.Type == ClaimTypes.Email)?.Value,
                    tip = Tvrdnja.FirstOrDefault(p => p.Type == ClaimTypes.Role)?.Value
                };
            }
            return null;
        }


        [Route("AddOglas/{naziv}/{tehnologija}/{opis}/{plata}/{remote_work}/{lokacija}/{korisnicko_ime}")]
        [HttpPost]
        public async Task<ActionResult> AddOglas(string naziv, string tehnologija, string opis, int plata, string remote_work, string lokacija, string korisnicko_ime)
        {
            if(string.IsNullOrWhiteSpace(tehnologija) || tehnologija.Length > 150)
                return BadRequest("Tehnologija prelazi opseg");

            if(string.IsNullOrWhiteSpace(opis) || opis.Length > 1500)
                return BadRequest("Opis prelazi opseg");   

            if(string.IsNullOrWhiteSpace(remote_work) || remote_work.Length >2)
                return BadRequest("Nevalidno remote work");   

            if(remote_work.CompareTo("Da")!=0 && remote_work.CompareTo("da")!=0 && remote_work.CompareTo("ne")!=0 && remote_work.CompareTo("Ne")!=0)
                return BadRequest("Nevalidno"); 
            var korisnik = await Context.Korisnici
                            .Where(p=>p.korisnicko_ime==korisnicko_ime).FirstOrDefaultAsync();
            if(korisnik==null) return BadRequest("Korisnik ne postoji");
           if(korisnik.tip.CompareTo("Radnik")==0) return BadRequest("Korisnik je Radnik");
            try
            {  
                  Oglasi o=new Oglasi{
                    naziv=naziv.Replace("01abfc750a0c942167651c40d088531d","#"),
                    tehnologija=tehnologija.Replace("01abfc750a0c942167651c40d088531d","#"),
                    opis=opis.Replace("01abfc750a0c942167651c40d088531d","#"),
                    plata=plata,
                    lokacija=lokacija.Replace("01abfc750a0c942167651c40d088531d",""),
                    remote_work=remote_work.Replace("01abfc750a0c942167651c40d088531d",""),
                    Korisnik=korisnik,
                    vreme = DateTime.Today,
                    };
                    Context.Oglasi.Add(o);
                    await Context.SaveChangesAsync();
                    return Ok("Uspesno dodat oglas");
     
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("PrikaziOglase")]
        [HttpGet]
        [AllowAnonymous]
      
        public async Task<ActionResult> PrikaziOglase()
        {
            var user = HttpContext.User;
            string vreme;
            int i=0;
            var oglasi = await Context.Oglasi.Include(p=>p.Korisnik)
                                             .ToListAsync();
            List<Poslodavac> korisnici = new List<Poslodavac>();
            foreach(Oglasi o in oglasi)
            {
                var kor = Context.Korisnici.Where(p=>p.ID==o.Korisnik.ID).FirstOrDefault();
                var posl = Context.Poslodavci.Where(p=>p.Korisnik.ID==kor.ID).FirstOrDefault();
                korisnici.Add(posl);
            }
            try{

                return Ok(
                            oglasi.Select(p =>
                            new
                            { 
                                p.ID,
                                p.naziv,
                                p.tehnologija,
                                p.opis,
                                p.plata,
                                p.remote_work,
                                p.lokacija,
                                vreme = p.vreme.ToString("dd/MM/yyyy"),
                                korisnici[i].naziv_firme,
                                korisnici[i].ime,
                                korisnici[i++].prezime
                                
                            }).ToList()
                        );
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }

        }

      
        [Route("GetOglasi/{tehnologija}/{minPlata}/{maxPlata}/{remote_work}/{lokacija}/{kljucna_rec}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetOglasi(string tehnologija, int minPlata, int maxPlata, string remote_work,string lokacija, string kljucna_rec)
        {
            string vreme;
            Korisnik k = VratiKorisnika();
            Korisnik Obj = Context.Korisnici.FirstOrDefault(p=> p.korisnicko_ime == k.korisnicko_ime);

            int a=1,b=1,c=0;
            if(string.IsNullOrWhiteSpace(tehnologija) || tehnologija.CompareTo("null")==0) a=0;
            if(string.IsNullOrWhiteSpace(remote_work) || remote_work.CompareTo("null")==0)  b=0;
            tehnologija=tehnologija.Replace("01abfc750a0c942167651c40d088531d","#");
            kljucna_rec=kljucna_rec.Replace("01abfc750a0c942167651c40d088531d","#");
            if(minPlata>0 && maxPlata>0) c=1;
            if(minPlata>0 && maxPlata==0) {maxPlata=Int32.MaxValue; c=1;}
             if(minPlata==0 && maxPlata>0) {c=1;}
            var o=await Context.Oglasi
                                            .Select(p=>p.naziv).ToListAsync();
            var o1=await Context.Oglasi
                                            .Select(p=>p.opis).ToListAsync();
            string sPattern = kljucna_rec;//ako hocemo bilo koja rec u text izbaciti ^
            List<string> lista=new List<string>();
            List<string> lista_oglasa_nazivi=new List<string>(); 
             

                foreach (string s in o1)
                {
                   
                   if ( string.IsNullOrWhiteSpace(kljucna_rec) || kljucna_rec.CompareTo("null")==0)
                    {
                         var pom= await Context.Oglasi.Where(p=>p.opis==s)
                                                        .Select(p=>p.naziv)
                                                        .FirstOrDefaultAsync();
                            lista.Add(pom); 
                    }
                    else {
                        if(System.Text.RegularExpressions.Regex.IsMatch(s, sPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase)) 
                        {
                            var pom= await Context.Oglasi.Where(p=>p.opis==s)
                                                        .Select(p=>p.naziv)
                                                        .FirstOrDefaultAsync();
                            lista.Add(pom); 
                        }
                    }
                }

             List<Oglasi> oglasi= new List<Oglasi>();
             List<Poslodavac> kor= new List<Poslodavac>();
            if(string.IsNullOrWhiteSpace(lokacija) || lokacija.CompareTo("null")==0)
            {
                
               
                foreach (string s in lista)
                {
                    switch(a,b,c)
                    {
                        case (0,0,0): var p0 = await Context.Oglasi
                                                        .Include(p=>p.Korisnik)
                                                        .Where(p=>p.naziv.ToLower()==s.ToLower())
                                                        .FirstOrDefaultAsync();
                                                        oglasi.Add(p0);
                                                        if(p0==null){}
                                                        else{
                                                          var korisnik0 = p0.Korisnik;
                                                       kor.Add(Context.Poslodavci.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID==korisnik0.ID).FirstOrDefault());
                                                        }
                                                        break;
                        case (0,0,1): var p1 = await Context.Oglasi
                                                        .Include(p=>p.Korisnik)
                                                        .Where(p=>p.plata>=minPlata && p.plata<=maxPlata && p.naziv.ToLower()==s.ToLower()).FirstOrDefaultAsync(); 
                                                        oglasi.Add(p1);
                                                        if(p1==null){}
                                                        else{
                                                        var korisnik1 = p1.Korisnik;
                                                       kor.Add(Context.Poslodavci.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID==korisnik1.ID).FirstOrDefault());
                                                        }
                                                        break;
                        case (0,1,0): var p2 = await Context.Oglasi
                                                        .Include(p=>p.Korisnik)
                                                        .Where(p=>p.remote_work.ToLower()==remote_work.ToLower() && p.naziv.ToLower()==s.ToLower()).FirstOrDefaultAsync();
                                                        oglasi.Add(p2);
                                                        if(p2==null) {}
                                                        else {Korisnik korisnik2 = p2.Korisnik;
                                                         kor.Add(Context.Poslodavci.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID==korisnik2.ID).FirstOrDefault());
                                                        }
                                                        break;
                        case (0,1,1): var p3 = await Context.Oglasi
                                                        .Include(p=>p.Korisnik)
                                                        .Where(p=>p.remote_work.ToLower()==remote_work.ToLower() && p.plata>=minPlata && p.plata<=maxPlata && p.naziv.ToLower()==s.ToLower()).FirstOrDefaultAsync();
                                                        oglasi.Add(p3);
                                                        if(p3==null){}
                                                        else{
                                                        var korisnik3 = p3.Korisnik;
                                                       kor.Add(Context.Poslodavci.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID==korisnik3.ID).FirstOrDefault());
                                                        }
                                                        break;
                        case (1,0,0): var p4 = await Context.Oglasi
                                                        .Include(p=>p.Korisnik)
                                                        .Where(p=>p.tehnologija.ToLower()==tehnologija.ToLower()  && p.naziv.ToLower()==s.ToLower()).FirstOrDefaultAsync();
                                                        oglasi.Add(p4);
                                                        if(p4==null){}
                                                        else{
                                                        var korisnik4 = p4.Korisnik;
                                                       kor.Add(Context.Poslodavci.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID==korisnik4.ID).FirstOrDefault());
                                                        }
                                                        break;
                        case (1,0,1): var p5 = await Context.Oglasi
                                                        .Include(p=>p.Korisnik)
                                                        .Where(p=>p.tehnologija.ToLower()==tehnologija.ToLower() && p.plata>=minPlata && p.plata<=maxPlata && p.naziv.ToLower()==s.ToLower()).FirstOrDefaultAsync();
                                                        oglasi.Add(p5);
                                                        if(p5==null){}
                                                        else{
                                                        var korisnik5 = p5.Korisnik;
                                                       kor.Add(Context.Poslodavci.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID==korisnik5.ID).FirstOrDefault());
                                                        }
                                                        break;
                        case (1,1,0): var p6 = await Context.Oglasi
                                                        .Include(p=>p.Korisnik)
                                                        .Where(p=>p.tehnologija.ToLower()==tehnologija.ToLower() && p.remote_work.ToLower()==remote_work.ToLower() && p.naziv.ToLower()==s.ToLower()).FirstOrDefaultAsync();
                                                        oglasi.Add(p6);
                                                        if(p6==null){}
                                                        else{
                                                        var korisnik6 = p6.Korisnik;
                                                       kor.Add(Context.Poslodavci.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID==korisnik6.ID).FirstOrDefault());
                                                        }
                                                        break;
                        case (1,1,1): var p7 = await Context.Oglasi
                                                        .Include(p=>p.Korisnik)
                                                        .Where(p=>p.tehnologija.ToLower()==tehnologija.ToLower() && p.remote_work.ToLower()==remote_work.ToLower() && p.plata>=minPlata && p.plata<=maxPlata  && p.naziv.ToLower()==s.ToLower()).FirstOrDefaultAsync();
                                                        oglasi.Add(p7);
                                                        if(p7==null){}
                                                        else{
                                                        var korisnik7 = p7.Korisnik;
                                                       kor.Add(Context.Poslodavci.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID==korisnik7.ID).FirstOrDefault());
                                                        }
                                                        break;
                        default: return BadRequest("Nedovoljno informacija da bi se filtriralo");
                    } 
                }  
                oglasi.RemoveAll(p=>p==null);

                int i=0;
                  return Ok(
                            oglasi.Select(p =>
                            new
                            { 
                                p.ID,
                                p.naziv,
                                p.tehnologija,
                                p.opis,
                                p.plata,
                                p.remote_work,
                                p.lokacija,
                                vreme = p.vreme.ToString("dd/MM/yyyy"),
                                kor[i].ime,
                                kor[i].prezime,
                                kor[i++].naziv_firme,
                                p.Korisnik.korisnicko_ime
                            }).ToList()
                        );
            }
            else {
               foreach (string s in lista)
                {
                    switch(a,b,c)
                    {
                        
                        case (0,0,0): var p0 = await Context.Oglasi
                                                        .Include(p=>p.Korisnik)
                                                        .Where(p=>p.lokacija.ToLower()==lokacija.ToLower() && p.naziv.ToLower()==s.ToLower()).FirstOrDefaultAsync();
                                                        if(p0!=null)
                                                        oglasi.Add(p0);
                                                         if(p0==null){}
                                                        else{
                                                          var korisnik0 = p0.Korisnik;
                                                       kor.Add(Context.Poslodavci.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID==korisnik0.ID).FirstOrDefault());
                                                        }
                                                        break;
                        case (0,0,1): var p1 = await Context.Oglasi
                                                        .Include(p=>p.Korisnik)
                                                        .Where(p=>p.plata>=minPlata && p.plata<=maxPlata && p.naziv.ToLower()==s.ToLower() && p.lokacija.ToLower()==lokacija.ToLower()).FirstOrDefaultAsync(); 
                                                        if(p1!=null)
                                                        oglasi.Add(p1);
                                                        if(p1==null){}
                                                        else{
                                                        var korisnik1 = p1.Korisnik;
                                                       kor.Add(Context.Poslodavci.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID==korisnik1.ID).FirstOrDefault());
                                                        }
                                                        break;
                        case (0,1,0): var p2 = await Context.Oglasi
                                                        .Include(p=>p.Korisnik)
                                                        .Where(p=>p.remote_work.ToLower()==remote_work.ToLower() && p.naziv.ToLower()==s.ToLower() && p.lokacija.ToLower()==lokacija.ToLower()).FirstOrDefaultAsync();
                                                        if(p2!=null)
                                                        oglasi.Add(p2);
                                                        if(p2==null) {}
                                                        else {Korisnik korisnik2 = p2.Korisnik;
                                                         kor.Add(Context.Poslodavci.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID==korisnik2.ID).FirstOrDefault());
                                                        }
                                                        break;
                        case (0,1,1): var p3 = await Context.Oglasi
                                                        .Include(p=>p.Korisnik)
                                                        .Where(p=>p.remote_work.ToLower()==remote_work && p.plata>=minPlata && p.plata<=maxPlata && p.naziv.ToLower()==s.ToLower() && p.lokacija.ToLower()==lokacija.ToLower()).FirstOrDefaultAsync();
                                                        if(p3!=null)
                                                        oglasi.Add(p3);
                                                        if(p3==null){}
                                                        else{
                                                        var korisnik3 = p3.Korisnik;
                                                       kor.Add(Context.Poslodavci.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID==korisnik3.ID).FirstOrDefault());
                                                        }
                                                        break;
                        case (1,0,0): var p4 = await Context.Oglasi
                                                        .Include(p=>p.Korisnik)
                                                        .Where(p=>p.tehnologija.ToLower()==tehnologija.ToLower() && p.lokacija.ToLower()==lokacija.ToLower() &&  p.naziv.ToLower()==s.ToLower()).FirstOrDefaultAsync();
                                                        if(p4!=null)
                                                        oglasi.Add(p4);
                                                        if(p4==null){}
                                                        else{
                                                        var korisnik4 = p4.Korisnik;
                                                       kor.Add(Context.Poslodavci.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID==korisnik4.ID).FirstOrDefault());
                                                        }
                                                        break;
                        case (1,0,1): var p5 = await Context.Oglasi
                                                        .Include(p=>p.Korisnik)
                                                        .Where(p=>p.tehnologija.ToLower()==tehnologija.ToLower() && p.plata>=minPlata && p.plata<=maxPlata && p.naziv.ToLower()==s.ToLower() && p.lokacija.ToLower()==lokacija.ToLower()).FirstOrDefaultAsync();
                                                        if(p5!=null)
                                                        oglasi.Add(p5);
                                                         if(p5==null){}
                                                        else{
                                                        var korisnik5 = p5.Korisnik;
                                                       kor.Add(Context.Poslodavci.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID==korisnik5.ID).FirstOrDefault());
                                                        }
                                                        break;
                        case (1,1,0): var p6 = await Context.Oglasi
                                                        .Include(p=>p.Korisnik)
                                                        .Where(p=>p.tehnologija.ToLower()==tehnologija.ToLower() && p.remote_work.ToLower()==remote_work.ToLower() && p.naziv.ToLower()==s.ToLower() && p.lokacija.ToLower()==lokacija.ToLower()).FirstOrDefaultAsync();
                                                        if(p6!=null)
                                                        oglasi.Add(p6);
                                                        if(p6==null){}
                                                        else{
                                                        var korisnik6 = p6.Korisnik;
                                                       kor.Add(Context.Poslodavci.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID==korisnik6.ID).FirstOrDefault());
                                                        }
                                                        break;
                        case (1,1,1): var p7 = await Context.Oglasi
                                                        .Include(p=>p.Korisnik)
                                                        .Where(p=>p.tehnologija==tehnologija && p.remote_work.ToLower()==remote_work && p.plata>=minPlata && p.plata<=maxPlata  && p.naziv==s && p.lokacija==lokacija ).FirstOrDefaultAsync();
                                                        if(p7!=null)
                                                        oglasi.Add(p7);
                                                        if(p7==null){}
                                                        else{
                                                        var korisnik7 = p7.Korisnik;
                                                       kor.Add(Context.Poslodavci.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID==korisnik7.ID).FirstOrDefault());
                                                        }
                                                        break;
                        default: return BadRequest("Nedovoljno informacija da bi se filtriralo");
                    }
                }
                 oglasi.RemoveAll(p=>p==null);
                 
                int i=0;
                  return Ok(
                            oglasi.Select(p =>
                            new
                            { 
                                p.ID,
                                p.naziv,
                                p.tehnologija,
                                p.opis,
                                p.plata,
                                p.remote_work,
                                p.lokacija,
                                vreme = p.vreme.ToString("dd/MM/yyyy"),
                                kor[i].ime,
                                kor[i].prezime,
                                kor[i++].naziv_firme,
                                p.Korisnik.korisnicko_ime
                            }).ToList()
                        );
            }
        }

        [Route("GetOglas/{korisnicko_ime}")]
        [HttpGet]
        public async Task<ActionResult> GetOglas(string korisnicko_ime)
        {

            var oglas = Context.Oglasi
            .Where(p => p.Korisnik.korisnicko_ime == korisnicko_ime).FirstOrDefault();
            
            if(oglas == null) return BadRequest("Ne postoji korisnik");

            var oglasi = await Context.Oglasi.Include(p=>p.Korisnik).Where(p => p.Korisnik.korisnicko_ime == korisnicko_ime).ToListAsync();
            var oglasi2 = await Context.Oglasi.Include(p=>p.Korisnik).Where(p => p.Korisnik.korisnicko_ime == korisnicko_ime).FirstOrDefaultAsync();
            var korisnik = await Context.Korisnici.Where(p=>p.ID == oglasi2.Korisnik.ID).FirstOrDefaultAsync();
            if(korisnik.tip.CompareTo("Poslodavac")==0)
            {
                var poslodavac = await Context.Poslodavci.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID == korisnik.ID).FirstOrDefaultAsync();
                if(poslodavac.tip_poslodavca.CompareTo("Firma")==0)
                {
                    return Ok(
                        oglasi.Select(p =>
                        new
                        { 
                            p.ID,
                            p.naziv,
                            p.tehnologija,
                            p.opis,
                            p.plata,
                            p.remote_work,
                            p.lokacija,
                            poslodavac.naziv_firme,
                            p.Korisnik.korisnicko_ime
                        }).ToList()
                    );
                }
                else
                {
                    return Ok(
                        oglasi.Select(p =>
                        new
                        { 
                            p.ID,
                            p.naziv,
                            p.tehnologija,
                            p.opis,
                            p.plata,
                            p.remote_work,
                            p.lokacija,
                            poslodavac.ime,
                            poslodavac.prezime,
                            p.Korisnik.korisnicko_ime
                        }).ToList()
                    );
                }
            }
            else return Ok(false);
        }

        [Route("ChangeOglas/{ID}/{naziv}/{opis}/{tehnologija}/{lokacija}/{remote_work}/{plata}")]
        [HttpPut]
        public async Task<ActionResult> ChangeOglas(int ID, string naziv , string opis, string tehnologija, string lokacija, string remote_work, int plata)
        {
           
             if(string.IsNullOrWhiteSpace(naziv) || naziv.Length > 150)
                 return BadRequest("Naziv prelazi opseg");

            if(string.IsNullOrWhiteSpace(tehnologija) || tehnologija.Length > 150)
                return BadRequest("Tehnologija prelazi opseg");

            if(string.IsNullOrWhiteSpace(opis) || opis.Length > 1500)
                return BadRequest("Opis prelazi opseg");   
            
            if(string.IsNullOrWhiteSpace(lokacija))    
                return BadRequest("Neispravno uneta lokacija");
            
            if(string.IsNullOrWhiteSpace(remote_work) || remote_work.Length >2)
                return BadRequest("Nevalidno remote work");  
            
            if(plata<0)
                return BadRequest("Nevalidna plata"); 
            try
            {
               var noviOglas = await Context.Oglasi.Where(p => p.ID == ID).FirstOrDefaultAsync();
                if(noviOglas==null) return BadRequest("Ne postoji taj oglas");
                noviOglas.naziv=naziv.Replace("01abfc750a0c942167651c40d088531d","#");
                noviOglas.lokacija=lokacija.Replace("01abfc750a0c942167651c40d088531d","");
                noviOglas.tehnologija=tehnologija.Replace("01abfc750a0c942167651c40d088531d","#");
                noviOglas.plata=plata;
                noviOglas.remote_work=remote_work;
                noviOglas.opis=opis.Replace("01abfc750a0c942167651c40d088531d","#");
                await Context.SaveChangesAsync();
                return Ok("Oglas izmenjen");
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("DeleteOglas/{ID}")]
        [HttpDelete]
        public async Task<ActionResult> DeleteOglas(int ID)
        {
            try
            { 
                var zahtevi = await Context.Zahtevi.Include(p=>p.Oglasi).Where(p=>p.Oglasi.ID==ID).ToListAsync();
                if(zahtevi==null) {}
                else{
                    foreach(Zahtevi o in zahtevi)
                    {
                        Context.Zahtevi.Remove(o);
                    } 
                }
                var Oglas = await Context.Oglasi.Where(p => p.ID == ID).FirstOrDefaultAsync();
                if(Oglas==null) return BadRequest("Ne postoji taj oglas");
                Context.Oglasi.Remove(Oglas);
                await Context.SaveChangesAsync();
                return Ok(true);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        

        [Route("GetOglasNaslov/{ID}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetOglasNaslov(int ID)
        {
            var oglas = Context.Oglasi.FirstOrDefault(p => p.ID== ID);
            
            if(oglas == null) return BadRequest("Ne postoji oglas");
            string retVal = oglas.naziv;
            return Ok(retVal);

        }
        [Route("GetOglasID/{ID}")]
        [HttpGet]
        public async Task<ActionResult> GetOglasID(int ID)
        {
            var oglasi = await Context.Oglasi.Where(p => p.ID == ID).ToListAsync();

            return Ok(
                oglasi.Select(p =>
                new
                { 
                    p.ID,
                    p.naziv,
                    p.tehnologija,
                    p.opis,
                    p.plata,
                    p.remote_work,
                    p.lokacija,
                    vreme=p.vreme.ToString("dd/MM/yyyy"),
                }).ToList()
            );
        }
    }
}
