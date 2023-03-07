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
    
    public class KorisnikController : ControllerBase
    {
        public DvlHubContext Context { get; set ;}
        private IConfiguration _config;
        public KorisnikController(DvlHubContext context , IConfiguration config)
        {
            Context=context;
            _config=config;

        }

        [HttpPost]
        [Route("Upload/{korisnicko_ime}")]
        public async Task<IActionResult> Upload(string korisnicko_ime)
        {

            try
            {      
                Korisnik Obj = VratiKorisnika();
                Korisnik retVal = Context.Korisnici.Where(p=> p.korisnicko_ime == korisnicko_ime).FirstOrDefault();
                var formCollection = await Request.ReadFormAsync();
                var file = formCollection.Files.First();
                var folderName = Path.Combine("Resources", "Images");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    retVal.imgPath=dbPath;
                    await Context.SaveChangesAsync();
                    return Ok(new { dbPath });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }


        [Route("PrikaziKorisnike")]
        [HttpGet]
        public async Task<ActionResult> PrikaziKorisnike()
        {

            try{
                return Ok(await Context.Korisnici.Select(p=> new {
                    p.ID,
                    p.korisnicko_ime,
                    p.email,
                    p.tip,
                    p.grad,
                    p.imgPath,
                }).ToListAsync());
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }

        }


        private string Generate(Korisnik korisnik) // Generisanje tokena
        {
            var Kljuc = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var Podaci = new SigningCredentials(Kljuc, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.GivenName, korisnik.korisnicko_ime),
                new Claim(ClaimTypes.Email ,korisnik.email),
                new Claim(ClaimTypes.Role, korisnik.tip)
            };
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
               _config["Jwt:Audience"],
               claims,
               expires:DateTime.Now.AddMinutes(60),
               signingCredentials: Podaci);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private Korisnik Authenticate(string korisnicko_ime, string lozinka) // Autentifikacija
        {
            Korisnik Current = Context.Korisnici.FirstOrDefault(p => p.korisnicko_ime.ToLower() == korisnicko_ime.ToLower());
            

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(

            password: lozinka,

            salt: Current.salt_value,

            prf: KeyDerivationPrf.HMACSHA256,

            iterationCount: 100000,

            numBytesRequested: 256 / 8));

            
            if (Current != null && Current.lozinka == hashed)
                return Current;
            else
                return null;
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
      
        [Route("VratiInfo")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> VratiInfo()
        {
            Korisnik Obj = VratiKorisnika();
            Korisnik retVal = Context.Korisnici.FirstOrDefault(p=> p.email == Obj.email);
            if(retVal != null)
             {
             if(retVal.tip == "Radnik")
             {
                Radnik retVal2 = Context.Radnici.FirstOrDefault(p=> p.Korisnik.ID == retVal.ID);
                string[] arr = {retVal.korisnicko_ime,retVal.tip,retVal.email,retVal.grad,retVal2.ime,retVal2.prezime,retVal.imgPath};
                return Ok(arr);
             }
             else if (retVal.tip == "Poslodavac")
             {
                    Poslodavac PoslVal = Context.Poslodavci.FirstOrDefault(p=> p.Korisnik.ID == retVal.ID);
                    if(PoslVal.tip_poslodavca == "Privatno lice")
                    {
                        string[] arr = {retVal.korisnicko_ime,PoslVal.tip_poslodavca,retVal.email,retVal.grad,PoslVal.kontakt,PoslVal.ime,PoslVal.prezime,retVal.imgPath};
                    
                      return Ok(arr);
                    }
                    else if (PoslVal.tip_poslodavca == "Firma")
                    {
                        string[] arr = {retVal.korisnicko_ime,PoslVal.tip_poslodavca,retVal.email,retVal.grad,PoslVal.kontakt,PoslVal.naziv_firme,PoslVal.lokacija_firme,retVal.imgPath};
                      
                       return Ok(arr);
                    }
                     
             }
             else if(retVal.tip == "Admin")
             {
                Admin AdminVal = Context.Admini.FirstOrDefault(p=> p.Korisnik.ID == retVal.ID);
                string[] arr = {retVal.korisnicko_ime,retVal.tip,retVal.email,retVal.grad,AdminVal.ime,AdminVal.prezime,retVal.imgPath};
                return Ok(arr);
             }
             return Ok(null);
             }
            else
            return Ok(null);
        }
        
        [Route("VratiInfoID/{ID}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> VratiInfoID(int ID)
        {
          
            Korisnik retVal = Context.Korisnici.FirstOrDefault(p=> p.ID == ID);
            if(retVal != null)
             {
             if(retVal.tip == "Radnik")
             {
                Radnik retVal2 = Context.Radnici.FirstOrDefault(p=> p.Korisnik.ID == retVal.ID);
                string[] arr = {retVal.korisnicko_ime,retVal.tip,retVal.email,retVal.grad,retVal2.ime,retVal2.prezime,retVal.imgPath};
                return Ok(arr);
             }
             else if (retVal.tip == "Poslodavac")
             {
                    Poslodavac PoslVal = Context.Poslodavci.FirstOrDefault(p=> p.Korisnik.ID == retVal.ID);
                    if(PoslVal.tip_poslodavca == "Privatno lice")
                    {
                        string[] arr = {retVal.korisnicko_ime,PoslVal.tip_poslodavca,retVal.email,retVal.grad,PoslVal.kontakt,PoslVal.ime,PoslVal.prezime,retVal.imgPath};
                    
                      return Ok(arr);
                    }
                    else if (PoslVal.tip_poslodavca == "Firma")
                    {
                        string[] arr = {retVal.korisnicko_ime,PoslVal.tip_poslodavca,retVal.email,retVal.grad,PoslVal.kontakt,PoslVal.naziv_firme,PoslVal.lokacija_firme,retVal.imgPath};
                      
                       return Ok(arr);
                    }
                     
             }
             else if(retVal.tip == "Admin")
             {
                Admin AdminVal = Context.Admini.FirstOrDefault(p=> p.Korisnik.ID == retVal.ID);
                string[] arr = {retVal.korisnicko_ime,retVal.tip,retVal.email,retVal.grad,AdminVal.ime,AdminVal.prezime,retVal.imgPath};
                return Ok(arr);
             }
             return Ok(null);
             }
            else
            return Ok(null);
         
        }   


        [Route("VratiInfoEmail/{Email}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> VratiInfoID(string Email)
        {
          
            Korisnik retVal = Context.Korisnici.FirstOrDefault(p=>p.email == Email);
            if(retVal != null)
             {
             if(retVal.tip == "Radnik")
             {
                Radnik retVal2 = Context.Radnici.FirstOrDefault(p=> p.Korisnik.ID == retVal.ID);
                string[] arr = {retVal.korisnicko_ime,retVal.tip,retVal.email,retVal.grad,retVal2.ime,retVal2.prezime,retVal.imgPath};
                return Ok(arr);
             }
             else if (retVal.tip == "Poslodavac")
             {
                    Poslodavac PoslVal = Context.Poslodavci.FirstOrDefault(p=> p.Korisnik.ID == retVal.ID);
                    if(PoslVal.tip_poslodavca == "Privatno lice")
                    {
                        string[] arr = {retVal.korisnicko_ime,PoslVal.tip_poslodavca,retVal.email,retVal.grad,PoslVal.kontakt,PoslVal.ime,PoslVal.prezime,retVal.imgPath};
                    
                      return Ok(arr);
                    }
                    else if (PoslVal.tip_poslodavca == "Firma")
                    {
                        string[] arr = {retVal.korisnicko_ime,PoslVal.tip_poslodavca,retVal.email,retVal.grad,PoslVal.kontakt,PoslVal.naziv_firme,PoslVal.lokacija_firme,retVal.imgPath};
                      
                       return Ok(arr);
                    }
                     
             }
             else if(retVal.tip == "Admin")
             {
                Admin AdminVal = Context.Admini.FirstOrDefault(p=> p.Korisnik.ID == retVal.ID);
                string[] arr = {retVal.korisnicko_ime,retVal.tip,retVal.email,retVal.grad,AdminVal.ime,AdminVal.prezime,retVal.imgPath};
                return Ok(arr);
             }
             return Ok(null);
             }
            else
            return Ok(null);
         
        }   


        
        [Route("VratiMojCv")]
        [HttpGet]
        public async Task<ActionResult> VratiMojCv()
        {
            Korisnik Obj = VratiKorisnika();
            Korisnik TestVal = Context.Korisnici.FirstOrDefault(p=> p.email == Obj.email);
            CV retCV = Context.CV.FirstOrDefault(p => p.Radnik.Korisnik.ID == TestVal.ID);
            if(retCV != null)
            return Ok(retCV);
            else
            return Ok(null);
         
        }
        [Route("SaveCv")]
        [HttpPut]
        public async Task<ActionResult> SaveCv(CV Updated) // Pogledati
        {
            Korisnik Obj = VratiKorisnika();
            Korisnik TestVal = Context.Korisnici.FirstOrDefault(p=> p.email == Obj.email);
            CV retCV = Context.CV.FirstOrDefault(p => p.Radnik.Korisnik.ID == TestVal.ID);
            if(retCV != null)
            return Ok(retCV);
            else
            return Ok(null);
            await Context.SaveChangesAsync();
         
        }






        [Route("ProveriPostojanjePodataka/{korisnicko_ime}/{email}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ProveriPostojanjePodataka(string korisnicko_ime, string email)
        {
            try{
            
              Korisnik korisnicko_ime_check =  Context.Korisnici.FirstOrDefault(p => p.korisnicko_ime.ToLower() == korisnicko_ime.ToLower());
              Korisnik email_check =  Context.Korisnici.FirstOrDefault(p => p.email.ToLower() == email.ToLower());
              if(korisnicko_ime_check != null || email_check != null)
              return Ok(true);
              else
              return Ok(false);

            }
            catch(Exception e)
            {
               return Ok(false);
            }
        }

        [Route("ProveriTip/{korisnicko_ime}/{lozinka}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<string> ProveriTip(string korisnicko_ime, string lozinka)
        {
            try
            {
                Korisnik Korisnik = Authenticate(korisnicko_ime, lozinka);
                if(Korisnik != null)
                {
                    return Korisnik.tip.ToString();
                }
                else
                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [Route("Login/{korisnicko_ime}/{lozinka}")]      
        [HttpGet]
        [AllowAnonymous]
        public async Task<string> Login(string korisnicko_ime, string lozinka)
        {
            try
            {
                lozinka = lozinka.Replace("01abfc750a0c942167651c40d088531d","#");
                Korisnik Korisnik = Authenticate(korisnicko_ime, lozinka);
                if(Korisnik != null)
                {
                    var token = Generate(Korisnik);
                    return token;
                }
                else
                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

       



        [Route("AddKorisnik/{korisnicko_ime}/{lozinka}/{email}/{tip}/{grad}/{ime}/{prezime}/{tip_poslodavca}/{lokacijaFirme}/{nazivFirme}/{kontakt}")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> AddKorisnik(string korisnicko_ime, string lozinka, string email, int tip, string grad, string ime, string prezime, int tip_poslodavca, string lokacijaFirme, string nazivFirme, string kontakt)
        {
            if(tip>2 || tip<0)
                 return BadRequest("Tip nije validan");

            if( tip_poslodavca>2 || tip_poslodavca<0)
                 return BadRequest("Tip poslodavca nije validan");

            if(string.IsNullOrWhiteSpace(korisnicko_ime) || korisnicko_ime.Length > 30)
                return BadRequest("korisnicko ime nije validno");

             if(string.IsNullOrWhiteSpace(lozinka) ||  lozinka.Length > 30)
                return BadRequest("Lozinka nije validna");   

            if(string.IsNullOrWhiteSpace(email) ||  email.Length > 60)
                return BadRequest("Email nije validan");   

            if(string.IsNullOrWhiteSpace(grad))
                return BadRequest("Grad nije validno");

            if(string.IsNullOrWhiteSpace(ime) ||  ime.Length > 30)
                return BadRequest("Ime nije validna");   

            if(string.IsNullOrWhiteSpace(prezime) ||  prezime.Length > 60)
                return BadRequest("Prezime nije validano");   

            if(string.IsNullOrWhiteSpace(lokacijaFirme) || lokacijaFirme.Length > 30)
                return BadRequest("Lokacija nije validno");

            if(string.IsNullOrWhiteSpace(nazivFirme) ||  nazivFirme.Length > 30)
                return BadRequest("Naziv firme nije validan");      
           
            if(string.IsNullOrWhiteSpace(kontakt) ||  kontakt.Length > 50)
                return BadRequest("Kontakt poslodavca nije validan");   

            if(await Context.Korisnici.Where(p=>p.korisnicko_ime==korisnicko_ime).FirstOrDefaultAsync()!=null) return BadRequest("Korisnicko ime zauzeto");
            if(await Context.Korisnici.Where(p=>p.email==email).FirstOrDefaultAsync()!=null) return BadRequest("Email je zauzet");

            lozinka=lozinka.Replace("01abfc750a0c942167651c40d088531d","#");

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
               switch(tip)
               {
                   //radnik
                   case 0: 
                     Korisnik k=new Korisnik{
                        korisnicko_ime=korisnicko_ime.Replace("01abfc750a0c942167651c40d088531d","#"),
                        lozinka=hashed,
                        email=email,
                        tip="Radnik",
                        grad=grad,
                        salt_value = salt,
                     };
                   Radnik r=new Radnik{
                       ime=ime.Replace("01abfc750a0c942167651c40d088531d",""),
                       prezime=prezime.Replace("01abfc750a0c942167651c40d088531d",""),
                       Korisnik=k,
                   };
                    Context.Korisnici.Add(k);
                    Context.Radnici.Add(r);
                    await Context.SaveChangesAsync();
                    return Ok(true);
                   //Poslodavac
                   case 1: 
                   
                        switch(tip_poslodavca){
                       
                         case 0:
                            Korisnik k1=new Korisnik{
                            korisnicko_ime=korisnicko_ime,
                            lozinka=hashed,
                            email=email,
                            tip="Poslodavac",
                            grad=grad,
                            salt_value = salt,
                            };
                            Poslodavac p=new Poslodavac{
                                ime=ime,
                                prezime=prezime,
                                kontakt=kontakt,
                                tip_poslodavca="Privatno lice",
                                Korisnik=k1,
                            };
                            Context.Korisnici.Add(k1);
                            Context.Poslodavci.Add(p);
                            await Context.SaveChangesAsync();
                            return Ok(true);

                         case 1: 
                            Korisnik k2=new Korisnik{
                            korisnicko_ime=korisnicko_ime,
                            lozinka=hashed,
                            email=email,
                            tip="Poslodavac",
                            grad=grad,
                            salt_value = salt,
                            };
                            Poslodavac p1=new Poslodavac{
                                naziv_firme=nazivFirme,
                                lokacija_firme=lokacijaFirme,
                                kontakt=kontakt,
                                Korisnik=k2,
                                tip_poslodavca="Firma",
                            };
                            Context.Korisnici.Add(k2);
                            Context.Poslodavci.Add(p1);
                            await Context.SaveChangesAsync();
                            return Ok(true);
                        default: return BadRequest("Greska!");
                        } 
                    default: return BadRequest("Greska!");
               }
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("ChangePassword/{email}/{lozinka}/{NovaLozinka}")]
        [HttpPut]
        [AllowAnonymous]
        public async Task<ActionResult> ChangeUserData(string email, string lozinka, string NovaLozinka)
        {
            
             if(string.IsNullOrWhiteSpace(email) || email.Length > 60)
                return BadRequest("korisnicko ime nije validno");

             if(string.IsNullOrWhiteSpace(lozinka) ||  lozinka.Length > 30)
                return BadRequest("Lozinka nije validna");   

             if(string.IsNullOrWhiteSpace(NovaLozinka) || NovaLozinka.Length > 30  && lozinka.CompareTo(NovaLozinka)==0)
                return BadRequest("Lozinka nije validna");  
                

            Korisnik Current = Context.Korisnici.FirstOrDefault(p => p.email.ToLower() == email.ToLower());
            lozinka=lozinka.Replace("01abfc750a0c942167651c40d088531d","#");

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(

            password: lozinka,

            salt: Current.salt_value,

            prf: KeyDerivationPrf.HMACSHA256,

            iterationCount: 100000,

            numBytesRequested: 256 / 8));

            if(Current.lozinka.CompareTo(hashed)==0)
            {
                byte[] salt = new byte[128 / 8];
                using (var rngCsp = new RNGCryptoServiceProvider())
                {
                    rngCsp.GetNonZeroBytes(salt);
                }
           
            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
                hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: NovaLozinka,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
                try
                {
                    var p = await Context.Korisnici.Where(p => p.email.ToLower() == email.ToLower() ).FirstOrDefaultAsync();
                    if(p==null) return Ok("Ne postoji taj korisnik");
                        p.lozinka=hashed;
                        p.salt_value = salt;
                    await Context.SaveChangesAsync();
                    return Ok(true);   
                } 
                catch(Exception e)
                {
                    return Ok(false);
                }
            }
            else return Ok();
        }
        
        [Route("ChangeUserInfo/{korisnicko_ime}/{tip}/{grad}/{ime}/{prezime}/{tip_poslodavca}/{lokacijaFirme}/{nazivFirme}/{kontakt}")]
        [HttpPut]
        [AllowAnonymous]
        public async Task<ActionResult> ChangeUserInfo(string korisnicko_ime, int tip, string grad, string ime, string prezime, int tip_poslodavca, string lokacijaFirme, string nazivFirme, string kontakt)
        {
           
            if(tip>3 || tip<0)
                 return BadRequest("Tip nije validan");

            if( tip_poslodavca>2 || tip_poslodavca<0)
                 return BadRequest("Tip poslodavca nije validan");
            
            if(string.IsNullOrWhiteSpace(korisnicko_ime) || korisnicko_ime.Length > 30)
                return BadRequest("korisnicko ime nije validno");  

            if(string.IsNullOrWhiteSpace(grad))
                return BadRequest("Grad nije validno");

            if(string.IsNullOrWhiteSpace(ime) ||  ime.Length > 30)
                return BadRequest("Ime nije validna");   

            if(string.IsNullOrWhiteSpace(prezime) ||  prezime.Length > 60)
                return BadRequest("Prezime nije validano");   

            if(string.IsNullOrWhiteSpace(lokacijaFirme) || lokacijaFirme.Length > 30)
                return BadRequest("Lokacija nije validno");

            if(string.IsNullOrWhiteSpace(nazivFirme) ||  nazivFirme.Length > 30)
                return BadRequest("Naziv firme nije validan");      
           
            if(string.IsNullOrWhiteSpace(kontakt) ||  kontakt.Length > 50)
                return BadRequest("Kontakt poslodavca nije validan");   
            
            try
            {
               var k = await Context.Korisnici.Where(p => p.korisnicko_ime == korisnicko_ime ).FirstOrDefaultAsync();
                if(k==null) return Ok(false);  
                k.korisnicko_ime=korisnicko_ime;
                k.grad=grad;
               
                switch(tip)
                {
                   //Radnik
                   case 0:
                    var r= await Context.Radnici.Where(p=>p.Korisnik==k).FirstOrDefaultAsync(); 
                    r.ime=ime;
                    r.prezime=prezime;
                    
                    await Context.SaveChangesAsync();
                    return Ok("Uspesno izmenjen radnik/korisnik");
                   
                   //Poslodavac
                   case 1: 

                        switch(tip_poslodavca){
                       
                         case 0:
                            var p1= await Context.Poslodavci.Where(p=>p.Korisnik==k).FirstOrDefaultAsync();
                            p1.kontakt=kontakt;
                            p1.ime=ime;
                            p1.prezime=prezime;
                            await Context.SaveChangesAsync();
                            return Ok("Uspesno izmenjen poslodavac/privatno lice/korisnik");

                         case 1: 
                            var p2= await Context.Poslodavci.Where(p=>p.Korisnik==k).FirstOrDefaultAsync();
                            p2.kontakt=kontakt;
                            p2.naziv_firme=nazivFirme;
                            p2.lokacija_firme=lokacijaFirme;
                            await Context.SaveChangesAsync();
                            return Ok("Uspesno izmenjen poslodavac/firma/korisnik");
                        default: return BadRequest("Greska!");
                        }
                    case 3:
                    var r1 = await Context.Admini.Where(p=>p.Korisnik==k).FirstOrDefaultAsync(); 
                    r1.ime=ime;
                    r1.prezime=prezime;      
                    await Context.SaveChangesAsync();
                    return Ok(true);
                    default: return BadRequest("Greska!");
               }
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [Route("PrikaziKorinika/{kljucna_rec}/{tip}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> PrikaziKorisnika(string kljucna_rec, string tip)
        {
            List<string> k1= new List<string>();
            List<Admin> admini = new List<Admin>();
            List<Radnik> radnici = new List<Radnik>();
            List<Korisnik> korisnici = new List<Korisnik>();
            List<Korisnik> korisnici3 = new List<Korisnik>();
            List<Korisnik> korisnici2 = new List<Korisnik>();
            List<Poslodavac> poslodavci = new List<Poslodavac>();      
            try{
                if(tip.CompareTo("null")==0)
                {
                    k1 = await Context.Korisnici
                    .Select(p=>p.email.ToLower()).ToListAsync();
                }
                else
                {
                    k1 = await Context.Korisnici
                        .Where(p=>p.tip==tip)    
                        .Select(p=>p.email.ToLower()).ToListAsync();
                }
                string sPattern = "^" +kljucna_rec.ToLower();//ako hocemo bilo koja rec u text izbaciti ^
                List<string> lista=new List<string>();
                foreach (string s in k1)
                {
                    if(kljucna_rec.CompareTo("null")==0)
                    {
                        lista.Add(s);
                    }
                    else 
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(s, sPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase) )
                        {
                            lista.Add(s);
                        }

                    }
                }
                if(tip.CompareTo("null")==0)
                {
                    foreach(string s in lista)
                    {   
                        var o = Context.Korisnici.Where(p=>p.email==s).FirstOrDefault();
                        
                        switch(o.tip)
                        {
                            case "Radnik":

                                var k = await Context.Korisnici.Where(p => p.email == o.email).FirstOrDefaultAsync();                             
                                korisnici.Add(k);

                            break;
                            case "Poslodavac": 
                                k = await Context.Korisnici.Where(p => p.email == o.email).FirstOrDefaultAsync(); 
                                korisnici.Add(k);
   
                            break;
                            case "Admin":
                                k = await Context.Korisnici.Where(p => p.email == o.email).FirstOrDefaultAsync();                    
                                korisnici.Add(k);
                            break;
                            default: return BadRequest("Greska!!");
                        }
                    }
                    return Ok( korisnici.Select(
                                p=> new{
                                korisnicko_ime= p.korisnicko_ime,
                                email = p.email,
                                grad = p.grad,
                                tip = p.tip,
                                ID = p.ID,
                                imgPath = p.imgPath
                                }
                            ).ToList());

                }
                if(tip.CompareTo("null")==0)
                {
                    foreach(string s in lista)
                    {   
                        var o = Context.Korisnici.Where(p=>p.email==s).FirstOrDefault();
                        
                        switch(o.tip)
                        {
                            case "Radnik":

                                    var k = await Context.Korisnici.Where(p => p.email == o.email).FirstOrDefaultAsync();                             
                                korisnici.Add(k);

                            break;
                            case "Poslodavac": 
                                    k = await Context.Korisnici.Where(p => p.email == o.email).FirstOrDefaultAsync(); 
                                            korisnici.Add(k);
   
                            break;
                            case "Admin":
                                    k = await Context.Korisnici.Where(p => p.email == o.email).FirstOrDefaultAsync();                    
                                korisnici.Add(k);
                            break;
                            default: return BadRequest("Greska!!");
                        }
                    }
                    return Ok( korisnici.Select(
                                p=> new{
                                korisnicko_ime= p.korisnicko_ime,
                                email = p.email,
                                grad = p.grad,
                                tip = p.tip,
                                ID = p.ID,
                                imgPath = p.imgPath
                                }
                            ).ToList());

                }

                       
                 switch(tip)
                {
                   //Radnik
                   case "Radnik":
                   
                    foreach(string s1 in lista)
                   {
                        var k = await Context.Korisnici.Where(p => p.email == s1).FirstOrDefaultAsync(); 
                        var r= Context.Radnici.Where(p=>p.Korisnik.email==s1).FirstOrDefault(); 
                        
                      korisnici.Add(k);
                      radnici.Add(r);
                   }
                   
                   int i=0;
                   return Ok( korisnici.Select(
                        p=> new{
                        korisnicko_ime= p.korisnicko_ime,
                        ime=radnici[i].ime,
                        prezime=radnici[i++].prezime,
                        email = p.email,
                        grad = p.grad,
                        tip = p.tip,
                        imgPath = p.imgPath
                        }
                    ).ToList());
                  
                   //Poslodavac
                    case "Poslodavac": 
                     foreach(string s1 in lista)
                    {
                        var k3 = await Context.Korisnici.Where(p => p.email == s1).FirstOrDefaultAsync(); 
                        var p1= Context.Poslodavci.Where(p=>p.Korisnik.email==s1).FirstOrDefault(); 	
								  korisnici2.Add(k3);
								  poslodavci.Add(p1);
							
                     
                    } 
                    
                    int j=0;
                   return Ok(korisnici2.Select(
                        p=> new{
                        korisnicko_ime= p.korisnicko_ime,
                        ime=poslodavci[j].ime,
                        prezime=poslodavci[j].prezime,
						kontakt = poslodavci[j].kontakt,
						lokacija_firme= poslodavci[j].lokacija_firme,
                        naziv_firme = poslodavci[j++].naziv_firme,
                        email = p.email,
                        grad = p.grad,
                        tip = p.tip,
                        imgPath = p.imgPath
                        }
                    ).ToList());
                    case "Admin":
                   
                    foreach(string s1 in lista)
                   {
                        var k2 = await Context.Korisnici.Where(p => p.email == s1).FirstOrDefaultAsync(); 
                        var a= Context.Admini.Where(p=>p.Korisnik.email==s1).FirstOrDefault(); 
                        
                      korisnici.Add(k2);
                      admini.Add(a);
                   }
                   
                   int l=0;
                   return Ok( korisnici.Select(
                        p=> new{
                        korisnicko_ime= p.korisnicko_ime,
                        ime=admini[l].ime,
                        prezime=admini[l++].prezime,
                        email = p.email,
                        grad = p.grad,
                        tip = p.tip,
                        imgPath = p.imgPath
                        }
                    ).ToList());
                    default: return BadRequest("Greska!");
               }
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [Route("GetName/{ID}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetName(int ID)
        {
            try
            {
                
                Korisnik Obj = await Context.Korisnici.Where(p=>p.ID == ID).FirstOrDefaultAsync();
                if(Obj == null) return Ok(false);
              
                switch(Obj.tip)
                {
                    case "Radnik":
                    Radnik RetVal = await Context.Radnici.Include(p=>p.Korisnik).Where(p=>p.Korisnik.ID == Obj.ID).FirstOrDefaultAsync();
                    string [] arr = {RetVal.ime,RetVal.prezime};
                    return Ok(arr); 

                   case "Poslodavac":
                   Poslodavac Obj2 =  Context.Poslodavci.Include(p=>p.Korisnik).FirstOrDefault(p => p.Korisnik.ID == Obj.ID);
                   switch(Obj2.tip_poslodavca)
                   {
                    case "Firma":
                    string [] arr2 = {Obj2.naziv_firme};
                    return Ok(arr2);

                    case "Privatno lice":
                    string [] arr3 = {Obj2.ime,Obj2.prezime};
                    return Ok(arr3);     
                    default:
                    break;
                   }
                   break;
                    default:
                    break;
                }
            
                return Ok(null);
            }
            catch (Exception e)
            {
                return null;
            }
        }   

        [Route("DeleteUsera/{id}")]
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUsera(int id)
        {
            try{
              
                var p1 = await Context.Korisnici.Where(q => q.ID ==id).FirstOrDefaultAsync();
              
              
                Context.Korisnici.Remove(p1);
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
