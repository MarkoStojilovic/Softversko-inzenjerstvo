export class Korisnik
{
    KorisnickoIme:string;
    Email:string; 
    Grad:string;
    Kontakt:string;
    Ime:string;
    Naziv:string;
    Prezime:string;
    Lokacija:string;
    Slika:string;

    email:string;
    tip:string;
    korisnicko_ime:string;
    ime:string;
    naziv:string;
    id:number;
    imgPath:string;
    token:string;
    
    constructor()
    {
        this.KorisnickoIme = "";
        this.Lokacija = ""; 
        this.Prezime = "";   
        this.Kontakt = "";
        this.Email = "";
        this.email = "";
        this.Naziv = "";
        this.Grad = "";  
        this.Ime = "";    
        this.ime = "";
        this.tip ="";
        this.Slika="";
        this.korisnicko_ime="";
        this.imgPath="-1"
        this.token = "";
    }
}