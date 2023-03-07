export class FilterOglas 
{
    //---Klasa koja obezbedjuje funkcionalnost filtriranja oglasa---//

    lokacija:string;
    max_plata:number;
    min_plata:number;
    kljucna_rec:string;
    tehnologija:string;
    remote_work:string;

    constructor()
    {   
        this.lokacija="";
        this.kljucna_rec="";
        this.tehnologija="";
        this.remote_work="";     
    }
    
}
