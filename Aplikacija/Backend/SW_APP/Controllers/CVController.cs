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

    [Authorize(Roles ="Radnik")]
    public class CVController : ControllerBase
    {
    public DvlHubContext Context { get; set ;}
    private IConfiguration _config;
        public CVController(DvlHubContext context , IConfiguration config)
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



      /*  [Route("AddCVAUTH/{obrazovanje}/{godina_iskustva}/{jezici}/{tehnologije}/{work_experience}/{licni_opis}/{email}/{adresa}/{telefon}")]
        [HttpGet]
        public async Task<ActionResult> AddCVAUTH(string obrazovanje, int godina_iskustva,string jezici, string tehnologije, string work_experience, string licni_opis, string email, string adresa, string telefon)
        {
           
            if(string.IsNullOrWhiteSpace(jezici))
                return BadRequest("Nevalidno");

            if(string.IsNullOrWhiteSpace(tehnologije))
                return BadRequest("Nevalidno");
 
            if(string.IsNullOrWhiteSpace(obrazovanje) || obrazovanje.Length > 150)
                return BadRequest("Nevalidno");
                
            if(godina_iskustva<0)
                return BadRequest("Godina izvan opsega");   

            if(string.IsNullOrWhiteSpace(email) || email.Length > 60)
                return BadRequest("Nevalidno");    

            if(string.IsNullOrWhiteSpace(adresa) || adresa.Length > 60)
                return BadRequest("Nevalidno");  

            if(string.IsNullOrWhiteSpace(telefon) || telefon.Length > 60)
                return BadRequest("Nevalidno");  

           

            Korisnik Tvrdnja = VratiKorisnika();
            Korisnik Korisnik = Context.Korisnici.FirstOrDefault(p => p.email == Tvrdnja.email);
             if(Tvrdnja.email == null)
             return Ok(true);
            Radnik Radnik = Context.Radnici.FirstOrDefault(p => p.Korisnik.ID == Korisnik.ID);
                           
           
            if(Radnik==null) return BadRequest("ne radi");
            if(Context.CV.FirstOrDefault(p => p.Radnik.ID == Radnik.ID) != null) 
           {
             CV  Del =  Context.CV.FirstOrDefault(p => p.Radnik.ID == Radnik.ID);
             Context.CV.Remove(Del);
           }
            try
            {  
                 CV c=new CV{
                    obrazovanje=obrazovanje,
                    godina_iskustva=godina_iskustva,
                    email=email,
                    adresa=adresa,
                    telefon=telefon,
                    jezici=jezici,
                    work_experience=work_experience,
                    tehnologije=tehnologije,
                    licni_opis=licni_opis,
                    Radnik=Radnik,
                    };
                    Context.CV.Add(c);
                    await Context.SaveChangesAsync();
                    return Ok("Uspesno dodat CV");
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }*/





        [Route("AddCV/{korisnicko_ime}/{obrazovanje}/{godina_iskustva}/{jezici}/{tehnologije}/{work_experience}/{licni_opis}/{email}/{adresa}/{telefon}")]
        [HttpPost]
        public async Task<ActionResult> AddCV(string korisnicko_ime,string obrazovanje, int godina_iskustva,string jezici, string tehnologije, string work_experience, string licni_opis, string email, string adresa, string telefon)
        {
            Korisnik obj = VratiKorisnika();
            if(string.IsNullOrWhiteSpace(jezici))
                return BadRequest("Nevalidno");

            if(string.IsNullOrWhiteSpace(tehnologije))
                return BadRequest("Nevalidno");
 
            if(string.IsNullOrWhiteSpace(obrazovanje) || obrazovanje.Length > 150)
                return BadRequest("Nevalidno");
                
            if(godina_iskustva<0)
                return BadRequest("Godina izvan opsega");   

            if(string.IsNullOrWhiteSpace(email) || email.Length > 60)
                return BadRequest("Nevalidno");    

            if(string.IsNullOrWhiteSpace(adresa) || adresa.Length > 60)
                return BadRequest("Nevalidno");  

            if(string.IsNullOrWhiteSpace(telefon) || telefon.Length > 60)
                return BadRequest("Nevalidno");  



            var radnik = await Context.Radnici
                            .Include(p=>p.Korisnik)
                            .Where(p=>p.Korisnik.korisnicko_ime==korisnicko_ime).FirstOrDefaultAsync();
            if(radnik==null) return BadRequest("ne radi");
             if(Context.CV.FirstOrDefault(p => p.Radnik.ID == radnik.ID) != null) 
           {
             CV  Del =  Context.CV.FirstOrDefault(p => p.Radnik.ID == radnik.ID);
             Context.CV.Remove(Del);
           }
            try
            {  
                 CV c=new CV{
                    obrazovanje=obrazovanje.Replace("01abfc750a0c942167651c40d088531d","#"),
                    godina_iskustva=godina_iskustva,
                    email=email.Replace("01abfc750a0c942167651c40d088531d",""),
                    adresa=adresa.Replace("01abfc750a0c942167651c40d088531d",""),
                    telefon=telefon.Replace("01abfc750a0c942167651c40d088531d",""),
                    jezici=jezici.Replace("01abfc750a0c942167651c40d088531d","#"),
                    work_experience=work_experience.Replace("01abfc750a0c942167651c40d088531d","#"),
                    tehnologije=tehnologije.Replace("01abfc750a0c942167651c40d088531d","#"),
                    licni_opis=licni_opis.Replace("01abfc750a0c942167651c40d088531d","#"),
                    Radnik=radnik,
                    };
                    Context.CV.Add(c);
                    await Context.SaveChangesAsync();
                    return Ok("Uspesno dodat CV");
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("ChangeCV/{korisnicko_ime}/{obrazovanje}/{godina_iskustva}/{jezici}/{tehnologije}/{work_experience}/{licni_opis}/{email}/{adresa}/{telefon}")]
        [HttpPut]
        public async Task<ActionResult> ChangeCV(string korisnicko_ime,string obrazovanje, int godina_iskustva,string jezici, string tehnologije, string work_experience, string licni_opis, string email, string adresa, string telefon)
        {
            try
            {
                var korisnik = await Context.CV
                            .Include(p=>p.Radnik)
                            .Include(p=>p.Radnik.Korisnik)
                            .Where(p=>p.Radnik.Korisnik.korisnicko_ime==korisnicko_ime).FirstOrDefaultAsync();
                if(korisnik==null) return BadRequest("Korisnik ne postoji");
                
                korisnik.obrazovanje=obrazovanje;
                korisnik.godina_iskustva=godina_iskustva;
                korisnik.email=email;
                korisnik.adresa=adresa;
                korisnik.telefon=telefon;
                korisnik.jezici=jezici;
                korisnik.tehnologije=tehnologije;
                korisnik.work_experience=work_experience;
                korisnik.licni_opis=licni_opis;
                await Context.SaveChangesAsync();
                return Ok("CV izmenjen");
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [Route("GetCV/{korisnicko_ime}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetCV(string korisnicko_ime)
        {
            try
            {
                var korisnik1 = Context.CV
                            .Include(p=>p.Radnik)
                            .Include(p=>p.Radnik.Korisnik)
                            .Where(p=>p.Radnik.Korisnik.korisnicko_ime==korisnicko_ime);
                
                if(korisnik1==null) return BadRequest("Korisnik ne postoji");
                var k = await korisnik1.ToListAsync();
                return Ok(
                k.Select(korisnik=>new {
                obrazovanje=korisnik.obrazovanje,
                godina_iskustva= korisnik.godina_iskustva,
                email=korisnik.email,
                adresa= korisnik.adresa,
                telefon=korisnik.telefon,
                jezici=korisnik.jezici,
                tehnologije=korisnik.tehnologije,
                work_experience=korisnik.work_experience,
                licni_opis=korisnik.licni_opis,
                }).ToList());
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [Route("GetCV2/{korisnicko_ime}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetCV2(string korisnicko_ime)
        {
            try
            {
                var korisnik1 = Context.CV
                            .Include(p=>p.Radnik)
                            .Include(p=>p.Radnik.Korisnik)
                            .Where(p=>p.Radnik.Korisnik.korisnicko_ime==korisnicko_ime);
                
                if(korisnik1==null) return BadRequest("Korisnik ne postoji");
                var k = await korisnik1.ToListAsync();
                CV CV = Context.CV.FirstOrDefault(p => p.Radnik.Korisnik.korisnicko_ime.ToLower() == korisnicko_ime.ToLower());
                string[] retVal ={CV.adresa,CV.email,CV.godina_iskustva+"",CV.jezici,CV.licni_opis,CV.obrazovanje,CV.tehnologije,CV.work_experience};
                
               // return Ok(retVal);
               return Ok(CV);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }

        }


    }
}
