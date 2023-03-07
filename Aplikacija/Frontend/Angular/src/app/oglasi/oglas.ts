export class Oglas 
{
    //---Klasa koja je neophodna za predstavljanje oglasa---/
    id:number;
    ime:string;
    opis:string;
    naziv:string;
    plata:number;  
    vreme:string;
    lokacija:string;
    prezime:string;
    tehnologija:string;
    remote_work:string;
    naziv_firme:string;
    
   
    constructor()
    {
        this.id=0;               
        this.ime="";
        this.opis="";
        this.plata=0;     
        this.vreme="";
        this.naziv="";
        this.prezime="";
        this.lokacija="";
        this.remote_work="";
        this.naziv_firme="";
        this.tehnologija="";
    }
}
