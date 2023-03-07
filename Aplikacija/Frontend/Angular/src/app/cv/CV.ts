export class CV
{  
    //---Klasa neophodna za rad za CV-jevima---//

    id:string;
    email:string;
    jezici:string;
    adresa:string;
    telefon:string;
    licni_opis:string;
    obrazovanje:string;
    tehnologije:string;
    work_experience:string;
    godina_iskustva:number;

    constructor()
    {
    this.godina_iskustva = 0;
    this.work_experience="";
    this.obrazovanje="";
    this.tehnologije="";
    this.licni_opis="";
    this.telefon="";
    this.adresa="";
    this.jezici="";
    this.email="";
    this.id="";
    }
}