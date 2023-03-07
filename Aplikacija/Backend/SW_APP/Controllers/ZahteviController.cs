using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using Microsoft.AspNetCore.Authorization;

namespace SW_APP.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Poslodavac,Radnik")]
    public class ZahteviController : ControllerBase
    {
    public DvlHubContext Context { get; set ;}
        public ZahteviController(DvlHubContext context)
        {
            Context=context;
        }

    [Route("Send/{ID_Posiljaoca}/{ID_Primaoca}/{IDoglasa}")]
    [HttpPost]
    public async Task<ActionResult> Send(int ID_Posiljaoca, int ID_Primaoca, int IDoglasa)
        {
                
            return Ok();
            var posiljaoc= await Context.Korisnici.Where(p=>p.ID==ID_Posiljaoca).FirstOrDefaultAsync();
            if(posiljaoc==null) return BadRequest("Ne postoji posiljaoc");

            var primaoc= await Context.Korisnici.Where(p=>p.ID==ID_Primaoca).FirstOrDefaultAsync();
            if(primaoc==null) return BadRequest("Ne postoji primaoc");

             var pr= await Context.Oglasi.Where(p=>p.ID==IDoglasa).FirstOrDefaultAsync();
            if(pr==null) return BadRequest("Ne postoji oglas");
            //-1 default
            //0 odbijanje
            //1 prihvatanje
            try
            {  
                  Zahtevi p =new Zahtevi{
                    status = -1,
                    ID_Posiljaoca=posiljaoc,
                    ID_Primaoca=primaoc,
                    Oglasi = pr
                    };
                    Context.Zahtevi.Add(p);
                    await Context.SaveChangesAsync();
                    return Ok("Uspesno poslat zahtev");
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    [Route("Send2/{KorisnickoIme}/{ID_Primaoca}/{IDoglasa}")]
    [HttpPost]
    public async Task<ActionResult> Send2(string korisnickoIme, int ID_Primaoca, int IDoglasa)
        {
            

            var posiljaoc= await Context.Korisnici.Where(p=>p.korisnicko_ime==korisnickoIme).FirstOrDefaultAsync();
            if(posiljaoc==null) return BadRequest("Ne postoji ovaj korisnik");

            var primaoc= await Context.Korisnici.Where(p=>p.ID==ID_Primaoca).FirstOrDefaultAsync();
            if(primaoc==null) return BadRequest("Ne postoji primaoc");

             var pr= await Context.Oglasi.Where(p=>p.ID==IDoglasa).FirstOrDefaultAsync();
            if(pr==null) return BadRequest("Ne postoji oglas");
            //-1 default
            //0 odbijanje
            //1 prihvatanje
            try
            {       var test = await Context.Zahtevi.Where(p=>p.Oglasi.ID == pr.ID && p.ID_Primaoca.ID == primaoc.ID).FirstOrDefaultAsync();
                    if(test != null) return BadRequest("Zahtev vec postoji");
                     var test3 = await Context.Zahtevi.Where(p=>p.Oglasi.ID==pr.ID && p.ID_Posiljaoca.ID==primaoc.ID && p.status==1).FirstOrDefaultAsync();
                    if(test3!=null) return BadRequest("Zahtev za ovaj oglas je vec prihvacen");
                    Zahtevi p =new Zahtevi{
                    status = -1,
                    ID_Posiljaoca=posiljaoc,
                    ID_Primaoca=primaoc,
                    Oglasi = pr
                    };
                    Context.Zahtevi.Add(p);
                    await Context.SaveChangesAsync();
                    return Ok(true);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }
           
        [Route("Get/{ID_Primaoca}")]
        [HttpPost]
        public async Task<ActionResult> Get(int ID_Primaoca)
        {

            var primaoc= await Context.Korisnici.Where(p=>p.ID==ID_Primaoca).FirstOrDefaultAsync();
            if(primaoc==null) return BadRequest("Ne postoji primaoc");
     
            try
            {  
                
                    return Ok(await Context.Zahtevi.Where(p=>p.ID_Primaoca.ID == ID_Primaoca)
                                                            .Select(p=> new{
                                                                p.Oglasi,
                                                                p.ID_Posiljaoca.korisnicko_ime,
                                                                p.ID,
                                                                p.status
                                                            })
                                                            .ToListAsync()

                    );
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("ChangeStatus/{IDZahteva}/{status}")]
        [HttpPut]
        public async Task<ActionResult> ChangeStatus(int IDZahteva, int status)
        {
          
            try
            {
                var zahtev = await Context.Zahtevi
                            .Where(p=>p.ID==IDZahteva).FirstOrDefaultAsync();
                if(zahtev==null) return BadRequest("Zahtev ne postoji");
                
                zahtev.status=status;
                
                await Context.SaveChangesAsync();
                return Ok("Status izmenjen");
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("DeleteZahtev/{ID}")]
        [HttpDelete]
        public async Task<ActionResult> DeleteZahtev(int ID)
        {
            try
            {
                var zahtev = await Context.Zahtevi.Where(p => p.ID == ID).FirstOrDefaultAsync();
                if(zahtev==null) return BadRequest("Ne postoji taj zahtev");
                Context.Zahtevi.Remove(zahtev);
                await Context.SaveChangesAsync();
                return Ok("Zahtev izbrisan");
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [Route("SendZahtev/{korisnicko_ime}/{id_oglasa}")]
        [HttpPost]
        public async Task<ActionResult> PosaljiSendZahtevZahtev(string korisnicko_ime,int id_oglasa)
        {
            
            var posiljaoc= await Context.Korisnici.Where(p=>p.korisnicko_ime==korisnicko_ime).FirstOrDefaultAsync();
            if(posiljaoc==null) return BadRequest("Ne postoji ovaj korisnik");

             var pr= await Context.Oglasi.Include(p=>p.Korisnik).Where(p=>p.ID==id_oglasa).FirstOrDefaultAsync();
            if(pr==null) return BadRequest("Ne postoji oglas");
            
            var primaoc = await Context.Korisnici.Where(p=>p.ID==pr.Korisnik.ID).FirstOrDefaultAsync();
            if(primaoc==null) return BadRequest("Ne postoji primaoc");

            //-1 default
            //0 odbijanje
            //1 prihvatanje
            try
            {   
               
                    var test = await Context.Zahtevi.Where(p=>p.Oglasi.ID == pr.ID && p.ID_Posiljaoca.ID == posiljaoc.ID).FirstOrDefaultAsync();
                  if(test != null) return BadRequest("Zahtev vec postoji");
                     var test3 = await Context.Zahtevi.Where(p=>p.Oglasi.ID==pr.ID && p.ID_Posiljaoca.ID==primaoc.ID && p.status==1).FirstOrDefaultAsync();
                    if(test3!=null) return BadRequest("Zahtev za ovaj oglas je vec prihvacen");
                   Zahtevi obj = new Zahtevi{
                    status = -1,
                    ID_Posiljaoca=posiljaoc,
                    ID_Primaoca=primaoc,
                    Oglasi = pr
                    };

            

                    Context.Zahtevi.Add(obj);
                    await Context.SaveChangesAsync();
                    return Ok(true);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        
        

        [Route("VratiListuZahteva/{korisnicko_ime}")]
        [HttpGet]
        public async Task<ActionResult> VratiListuZahteva(string korisnicko_ime)
        {
           
            try
            {
                var zahtevi = await Context.Zahtevi.Include(p=>p.ID_Primaoca).Include(p=>p.ID_Posiljaoca).Include(p=>p.Oglasi).Where(p => p.ID_Primaoca.korisnicko_ime == korisnicko_ime).ToListAsync();
                if(zahtevi==null) return BadRequest("Ne postoji taj zahtev");
                int n=0;
                foreach(var obj in zahtevi)
                {
                 n++;
                }

                string[] arr = new string[n*5];
                int iterator = 0;                        
                foreach(var obj in zahtevi)
                { 
                    if(obj.status == -1)                   
               {
                 arr[iterator++] = obj.ID+"";
                 arr[iterator++] = obj.status+"";
                 arr[iterator++] = obj.ID_Posiljaoca.ID+"";               
                 arr[iterator++] = obj.ID_Primaoca.ID+"";
                 arr[iterator++] = obj.Oglasi.ID+"";  
               }              
                }
              
                return Ok(arr);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [Route("VratiPotvrdjene/{korisnicko_ime}")]
        [HttpGet]
        public async Task<ActionResult> VratiPotvrdjene(string korisnicko_ime)
        {
            try
            {
                var zahtevi = await Context.Zahtevi.Include(p=>p.ID_Primaoca).Include(p=>p.ID_Posiljaoca).Include(p=>p.Oglasi).Where(p => p.ID_Posiljaoca.korisnicko_ime == korisnicko_ime).ToListAsync();
                if(zahtevi==null) return BadRequest("Ne postoji taj zahtev");
                int n=0;
                foreach(var obj in zahtevi)
                {
                 n++;
                }
                string[] arr = new string[n*5];
                int iterator = 0;                        
                foreach(var obj in zahtevi)
                { 
                    if(obj.status != -1)                   
               {
                 arr[iterator++] = obj.ID+"";
                 arr[iterator++] = obj.status+"";
                 arr[iterator++] = obj.ID_Posiljaoca.ID+"";               
                 arr[iterator++] = obj.ID_Primaoca.ID+"";
                 arr[iterator++] = obj.Oglasi.ID+"";  
               }              
                }
                return Ok(arr);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
