package com.example.qlikway

import android.os.Bundle
import android.support.design.widget.Snackbar
import android.support.v7.app.AppCompatActivity
import androidx.navigation.findNavController
import androidx.navigation.ui.AppBarConfiguration
import androidx.navigation.ui.navigateUp
import androidx.navigation.ui.setupActionBarWithNavController
import android.view.Menu
import android.view.MenuItem
import com.example.qlikway.databinding.ActivityMainBinding
import java.lang.Math
import java.lang.Math.ceil
import java.util.*

class MainActivity : AppCompatActivity()
{

    private lateinit var appBarConfiguration: AppBarConfiguration
    private lateinit var binding: ActivityMainBinding

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        binding = ActivityMainBinding.inflate(layoutInflater)
        setContentView(binding.root)

        setSupportActionBar(binding.toolbar)

        val navController = findNavController(R.id.nav_host_fragment_content_main)
        appBarConfiguration = AppBarConfiguration(navController.graph)
        setupActionBarWithNavController(navController, appBarConfiguration)

        binding.fab.setOnClickListener { view ->
            Snackbar.make(view, "Replace with your own action", Snackbar.LENGTH_LONG)
                    .setAction("Action", null).show()
        }
    }

    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        // Inflate the menu; this adds items to the action bar if it is present.
        menuInflater.inflate(R.menu.menu_main, menu)
        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        return when (item.itemId) {
            R.id.action_settings -> true
            else -> super.onOptionsItemSelected(item)
        }
    }

    override fun onSupportNavigateUp(): Boolean {
        val navController = findNavController(R.id.nav_host_fragment_content_main)
        return navController.navigateUp(appBarConfiguration)
                || super.onSupportNavigateUp()
    }


}

// Une classe pour représenter un emprunteur
class Emprunteur(val nom: String, var fiabilite: Double)
{

    // Une liste pour stocker les prêts en cours de cet emprunteur
    val prets = mutableListOf<Pret>()

    val conf = config()

    // Une méthode pour emprunter une somme d'argent
    fun emprunter(montant: Double, DateRemb: Date)
    {
        // Vérifier que le montant est inférieur ou égal à 100
        if (montant <= conf.montantMaxEmprunt)
        {
            // Créer un nouveau prêt avec le montant et la date actuelle
            val pret = Pret(montant, DateRemb)

            // Ajouter le prêt à la liste des prêts de l'emprunteur
            prets.add(pret)

            // Afficher un message de confirmation
            println("Vous avez emprunté $montant Fcfa.")
        }
        else
        {
            // Afficher un message d'erreur
            println("Vous ne pouvez pas emprunter plus de ${conf.montantMaxEmprunt} Fcfa.")
        }
    }
    // Une méthode pour rembourser un prêt
    fun rembourser(pret: Pret)
    {
        // Vérifier que le prêt appartient à l'emprunteur
        if (prets.contains(pret))
        {
            // Calculer le montant à rembourser avec l'intérêt de 5% par semaine
            val interet = pret.montant * 0.05 * pret.semaines
            val total = pret.montant + interet

            // Retirer le prêt de la liste des prêts de l'emprunteur
            prets.remove(pret)

            // Augmenter la fiabilité de l'emprunteur en fonction du montant remboursé
            fiabilite += total / 1000

            // Afficher un message de confirmation
            println("Vous avez remboursé $total dollars ($interet dollars d'intérêt).")
        }
        else
        {
            // Afficher un message d'erreur
            println("Ce prêt ne vous appartient pas.")
        }
    }
}

// Une classe pour représenter un prêt
class Pret(val montant: Double, var dateRemb: Date)
{
    var conf = config()
    // Une méthode pour calculer la durée du prêt en semaines
    fun CalcDuree(): Int
    {

        // Obtenir la date actuelle
        val aujourdHui = Date()

        // Calculer la différence en millisecondes entre les deux dates
        val difference = aujourdHui.time - dateRemb.time

        // Convertir la différence en semaines (en arrondissant au supérieur)
        val sem = ceil((difference / (1000 * 60 * 60 * 24 * 7)).toDouble()).toInt()

        // Retourner le nombre de semaines
        return sem
    }

    // nombre de semaines avant rembourssement exigé
    var semaines:Int = CalcDuree()

    //valeur actuelle du taux pour le prêt
    var tauxDuPret :Double = conf.tauxInteret

    //montant qu'il faudra rembourser
    var montantAremb:Int = ceil(montant * (1 + semaines.toDouble()
            * conf.tauxInteret/conf.dureeEmprunt.toDouble())).toInt()
}

// Une classe pour représenter un prêteur sur gage
class preteur(val nom: String)
{
    // Une liste pour stocker les emprunteurs connus
    val emprunteurs = mutableListOf<Emprunteur>()

    // Une méthode pour ajouter un nouvel emprunteur
    fun ajouterEmprunteur(nom: String) {
        // Vérifier que le nom n'est pas déjà dans la liste des emprunteurs
        if (emprunteurs.none { it.nom == nom })
        {
            // Créer un nouvel emprunteur avec une fiabilité initiale de 1.0
            val emprunteur = Emprunteur(nom, 1.0)

            // Ajouter l'emprunteur à la liste des emprunteurs
            emprunteurs.add(emprunteur)

            // Afficher un message de confirmation
            println("Vous avez ajouté $nom comme nouvel emprunteur.")
        }
        else
        {
            // Afficher un message d'erreur
            println("Ce nom est déjà dans la liste des emprunteurs.")
        }
    }

    // Une méthode pour trouver un emprunteur par son nom
    fun trouverEmprunteur(nom: String): Emprunteur?
    {
        // Parcourir la liste des emprunteurs et retourner celui qui a le nom recherché, ou null si aucun ne correspond
        for (emprunteur in emprunteurs){if (emprunteur.nom == nom) {return emprunteur}}
        return null
    }

    // Une méthode pour afficher les informations sur un emprunteur
    fun afficherEmprunteur(nom: String) {

        // Trouver l'emprunteur par son nom
        val clientEmprunteur = trouverEmprunteur(nom)

        // Vérifier que l'emprunteur existe
        if (clientEmprunteur != null) {

            // Afficher son nom et sa fiabilité
            println("Nom: ${clientEmprunteur.nom}")
            println("Fiabilité: ${clientEmprunteur.fiabilite}")

            // Afficher le nombre et le montant total de ses prêts en cours
            val nombrePrets = clientEmprunteur.prets.size
            val montantTotal = clientEmprunteur.prets.sumOf { it.montant }
            println("Nombre de prêts en cours: $nombrePrets")
            println("Montant total des prêts en cours: $montantTotal")
        }
        else
        {
            // Afficher un message d'erreur
            println("Aucun emprunteur ne correspond à ce nom.")
        }
    }
}

//Une classe pour les paramètres généraux de configuration métier
class config()
{
    val montantMaxEmprunt:Int = 50000  // Fcfa
    val tauxInteret : Double = 0.05    // Pourcent
    val dureeEmprunt : Int = 1         // Semaines (durée d'applicabilité du porcentage)
}