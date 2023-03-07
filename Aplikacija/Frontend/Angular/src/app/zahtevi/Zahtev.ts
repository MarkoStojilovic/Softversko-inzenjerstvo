export class Zahtev
{
    id:string;
    status:string;
    id_posiljaoca:string;
    id_primaoca:string;
    id_oglasa:string;

    ime_posiljaoca:string
    ime_primaoca:string;
    naziv_oglasa:string;

    constructor()
    {
        this.id = "";
        this.status = "";
        this.id_posiljaoca = "";
        this.id_primaoca = "";
        this.id_oglasa = "";  
        this.ime_posiljaoca = " ";
        this.ime_primaoca = "";
        this.naziv_oglasa = "";
    }
}