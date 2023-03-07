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
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
    public DvlHubContext Context { get; set ;}
    private IConfiguration _config;
        public AdminController(DvlHubContext context , IConfiguration config)
        {
            Context=context;
            _config=config;
        }

        [Route("AddAdmin/{korisnicko_ime}/{lozinka}/{email}/{ime}/{prezime}/{grad}")]
        [HttpPost]
        public async Task<ActionResult> AddAdmin(string korisnicko_ime,string lozinka, string email,string ime, string prezime, string grad)
        {
            byte[] salt = new byte[128 / 8];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetNonZeroBytes(salt);
            }
           
            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: lozinka,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            try
            {  
                Korisnik k=new Korisnik{
                    korisnicko_ime=korisnicko_ime,
                    lozinka=hashed,
                    email=email,
                    salt_value=salt,
                    grad=grad,
                    tip="Admin"
                };
                 Admin a=new Admin{
                     ime=ime,
                     prezime=prezime,
                     Korisnik=k
                    };
                    Context.Admini.Add(a);
                    Context.Korisnici.Add(k);
                    await Context.SaveChangesAsync();
                    return Ok("Uspesno dodat admin");
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }


      [Route("DeleteKorisnika/{korisnicko_ime}")]
      [HttpDelete]
        public async Task<ActionResult> DeleteKorisnika(string korisnicko_ime)
        {
            try{
            
            Korisnik Obj =  Context.Korisnici.FirstOrDefault(p => p.korisnicko_ime.ToLower() == korisnicko_ime.ToLower());
              
            if(Obj != null)
            {
                var zahtevi = await Context.Zahtevi.Include(p=>p.ID_Posiljaoca).Where(p=>p.ID_Posiljaoca.ID==Obj.ID).ToListAsync();
                if(zahtevi==null) {}
                else{
                    foreach(Zahtevi o in zahtevi)
                    {
                        Context.Zahtevi.Remove(o);
                    } 
                }
               
                var zahtevi2 = await Context.Zahtevi.Include(p=>p.ID_Primaoca).Where(p=>p.ID_Primaoca.ID==Obj.ID).ToListAsync();
                
                if(zahtevi2==null) {}
                else{
                    foreach(Zahtevi o in zahtevi2)
                    {
                        Context.Zahtevi.Remove(o);
                    }
                }

                var prMess = await Context.Private_Message.Include(p=>p.ID_Posiljaoca).Where(p=>p.ID_Posiljaoca.ID==Obj.ID).ToListAsync();
                if(prMess==null) {}
                else{
                    foreach(Private_Message o in prMess)
                    {
                        Context.Private_Message.Remove(o);
                    } 
                }
                var prMess2 = await Context.Private_Message.Include(p=>p.ID_Primaoca).Where(p=>p.ID_Primaoca.ID==Obj.ID).ToListAsync();
                if(prMess==null) {}
                else{
                    foreach(Private_Message o in prMess2)
                    {
                        Context.Private_Message.Remove(o);
                    }
                }
                switch(Obj.tip)
                {
                    case "Radnik":
                        var radnik = Context.Radnici.Include(p=>p.Korisnik).FirstOrDefault(p=>p.Korisnik.ID==Obj.ID);
                        CV cv = Context.CV.Include(p=>p.Radnik).FirstOrDefault(p=>p.Radnik.ID==radnik.ID);
                        if(cv==null){}
                        else  {Context.CV.Remove(cv);}
                        Context.Radnici.Remove(Context.Radnici.Include(p=>p.Korisnik).FirstOrDefault(p => p.Korisnik.ID == Obj.ID));
                    break;
                    case "Poslodavac": 
                        var Obj2 =  Context.Poslodavci.Include(p=>p.Korisnik).FirstOrDefault(p => p.Korisnik.ID == Obj.ID);

                        var oglasi2 = await Context.Oglasi.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID==Obj.ID).ToListAsync();
                        if(oglasi2==null) {}
                        else {
                            foreach(Oglasi o in oglasi2)
                            {
                                Context.Oglasi.Remove(o);
                            } 
                        }
                    switch(Obj2.tip_poslodavca)
                    {
                        case "Firma":
                            Context.Poslodavci.Remove(Context.Poslodavci.Include(p=>p.Korisnik).FirstOrDefault(p => p.Korisnik.ID == Obj.ID));
                        break;

                        case "Privatno lice":
                            Context.Poslodavci.Remove(Context.Poslodavci.Include(p=>p.Korisnik).FirstOrDefault(p => p.Korisnik.ID == Obj.ID));
                        break;

                        default:
                        break;
                    }
                   break;
                    case "Admin":
                        var admin = Context.Admini.Include(p=>p.Korisnik).FirstOrDefault(p=>p.Korisnik.ID==Obj.ID);
                        Context.Admini.Remove(admin);
                    break;
                    default:
                    break;
                }
                 
                Context.Korisnici.Remove(Obj);
                await Context.SaveChangesAsync();
                return Ok(true);
            }
              
              else
              return Ok(false);

            }
            catch(Exception e)
            {
               return Ok(e.ToString());
            }
        }








        [Route("ObrisiOglasAdmin/{idOglasa}/{kIme}")]
        [HttpDelete]
        public async Task<ActionResult> ObrisiOglas(int idOglasa,string kIme)
        {
            var oglas = await Context.Oglasi.Include(p=>p.Korisnik).Where(p=>p.ID==idOglasa).FirstOrDefaultAsync();
            if(oglas==null) return BadRequest("Ne postoji taj oglas");
            var primalac = oglas.Korisnik;
            //var primalac = await Context.Korisnici.Where(p=>oglas.Korisnik.korisnicko_ime == p.korisnicko_ime).FirstOrDefaultAsync();
             if(primalac==null) return BadRequest("Ne postoji primalac");
            var salje = await Context.Korisnici.Where(p=>p.korisnicko_ime==kIme).FirstOrDefaultAsync();
             if(salje==null) return BadRequest("Ne postoji posiljalac");
             var admin = await Context.Admini.Include(p=>p.Korisnik).Where(p=>p.Korisnik.korisnicko_ime==kIme).FirstOrDefaultAsync();
            string naslov = "Vas oglas je obrisan";
            string poruka = "Vas oglas " + oglas.naziv + " je obrisan od strane admina '" + admin.ime + " " +admin.prezime  + "' zato sto nije u skladu sa nasim smernicama";
             try
            {  
                 var zahtevi = await Context.Zahtevi.Include(p=>p.Oglasi).Where(p=>p.Oglasi.ID==idOglasa).ToListAsync();
                if(zahtevi==null) {}
                else{
                    foreach(Zahtevi o in zahtevi)
                    {
                        Context.Zahtevi.Remove(o);
                    } 
                }

                  Context.Oglasi.Remove(oglas);
                  Private_Message p =new Private_Message{
                    poruka=poruka,
                    naslov=naslov,
                    vreme_pristizanja=DateTime.Now,
                    ID_Posiljaoca=salje,
                    ID_Primaoca=primalac,
                    };
                    Context.Private_Message.Add(p);
                    await Context.SaveChangesAsync();
                    return Ok(true);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}