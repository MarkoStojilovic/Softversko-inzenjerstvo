export class Poruka
{

    //---Klasa neophodna za rad sa sistemom poruka---//
    id:number;
    email:string;
    datum:string;
    vreme:string;
    poruka:string;
    naslov:string;
    korisnicko_ime:string;

    constructor()
    {
        this.korisnicko_ime="";
        this.poruka="";
        this.naslov="";
        this.email=""; 
        this.datum="";
        this.vreme="";
        this.id=0;
    }
}