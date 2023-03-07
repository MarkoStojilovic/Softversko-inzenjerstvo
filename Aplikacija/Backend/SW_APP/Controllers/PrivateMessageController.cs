using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;

namespace SW_APP.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class PrivateMessageController : ControllerBase
    {
    public DvlHubContext Context { get; set ;}
        public PrivateMessageController(DvlHubContext context)
        {
            Context=context;
        }

    [Route("Send/{naslov}/{poruka}/{korisnicko_ime_Posiljaoca}/{email_Primaoca}")]
    [HttpPost]
    public async Task<ActionResult> Send(string naslov,string poruka, string korisnicko_ime_Posiljaoca, string email_Primaoca)
        {
            
            if(string.IsNullOrWhiteSpace(naslov) || naslov.Length > 100)
                return BadRequest("Greska u naslovu");

            if(string.IsNullOrWhiteSpace(poruka) || poruka.Length > 1000)
                return BadRequest("Poruka prelazi opseg");

            if(string.IsNullOrWhiteSpace(korisnicko_ime_Posiljaoca))
                return BadRequest("Nema email posiljaoca");

            if(string.IsNullOrWhiteSpace(email_Primaoca))
                return BadRequest("Nema email primaoca");

            var posiljaoc= await Context.Korisnici.Where(p=>p.korisnicko_ime==korisnicko_ime_Posiljaoca).FirstOrDefaultAsync();
            if(posiljaoc==null) return BadRequest("Ne postoji posiljaoc");

            var primaoc= await Context.Korisnici.Where(p=>p.email==email_Primaoca).FirstOrDefaultAsync();
            if(primaoc==null) return BadRequest("Ne postoji primaoc");
     
            try
            {  
                  Private_Message p =new Private_Message{
                    poruka=poruka.Replace("01abfc750a0c942167651c40d088531d","#"),
                    naslov=naslov.Replace("01abfc750a0c942167651c40d088531d","#"),
                    vreme_pristizanja=DateTime.Now,
                    ID_Posiljaoca=posiljaoc,
                    ID_Primaoca=primaoc,
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
           
        [Route("GetPoslate/{korisnicko_ime}")]
        [HttpGet]
        public async Task<ActionResult> GetPoslate(string korisnicko_ime)
        {
            var posiljaoc = await Context.Korisnici.Where(p=>p.korisnicko_ime==korisnicko_ime).FirstOrDefaultAsync();
            if(posiljaoc==null) return BadRequest("Ne postoji posiljaoc");
            string vreme;
            string datum;
            try
            {  
                
                    return Ok(await Context.Private_Message.Where(p=>p.ID_Posiljaoca.korisnicko_ime == posiljaoc.korisnicko_ime)
                                                            .Select(p=> new{
                                                                p.poruka,
                                                                p.naslov,
                                                                vreme=p.vreme_pristizanja.ToString("HH:mm:ss"),
                                                                datum = p.vreme_pristizanja.ToString("dd/MM/yyyy"),
                                                                p.ID_Primaoca.korisnicko_ime,
                                                                p.ID_Primaoca.email,
                                                                p.ID
                                                            })
                                                            .ToListAsync()

                    );
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [Route("GetPrimljene/{korisnicko_ime}")]
        [HttpGet]
        public async Task<ActionResult> GetPrimljene(string korisnicko_ime)
        {
            var primaoc= await Context.Korisnici.Where(p=>p.korisnicko_ime==korisnicko_ime).FirstOrDefaultAsync();
            if(primaoc==null) return BadRequest("Ne postoji primaoc");
            string vreme;
            string datum;
            try
            {  
                
                    return Ok(await Context.Private_Message.Where(p=>p.ID_Primaoca.korisnicko_ime == primaoc.korisnicko_ime)
                                                            .Select(p=> new{
                                                                p.poruka,
                                                                p.naslov,
                                                                vreme=p.vreme_pristizanja.ToString("HH:mm:ss"),
                                                                datum = p.vreme_pristizanja.ToString("dd/MM/yyyy"),
                                                                p.ID_Posiljaoca.korisnicko_ime,
                                                                p.ID_Posiljaoca.email,
                                                                p.ID
                                                            })
                                                            .ToListAsync()

                    );
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("DeleteMessage/{ID}")]
        [HttpDelete]
        public async Task<ActionResult> DeleteMessage(int ID)
        {
            try
            {
                var Poruka = await Context.Private_Message.Where(p => p.ID == ID).FirstOrDefaultAsync();
                if(Poruka==null) return BadRequest("Ne postoji ta poruka");
                Context.Private_Message.Remove(Poruka);
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
