using Newtonsoft.Json;
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Globalization;


namespace GUI_final_day
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SelectedPokemon.Root thisPokemon;
        int index = 0;

        public MainWindow()
        {
            InitializeComponent();

            next.IsEnabled = false;
            previous.IsEnabled = false;

            Gen_Pick();
        }

        // sets first character of a string to Upper case
        public string TitleCase(string passedString)
        {
            passedString = passedString.Remove(1).ToUpper() + passedString.Substring(1);
            return passedString;
        }

        // this needs to be selected before you can select from cb(pokemon selection box)
        private void Gen_Pick()
        {
            // limiter for generations
            string[] urls =
            {
                "https://pokeapi.co/api/v2/pokemon?limit=151",
                "https://pokeapi.co/api/v2/pokemon?limit=100&offset=151",
                "https://pokeapi.co/api/v2/pokemon?limit=135&offset=251",
                "https://pokeapi.co/api/v2/pokemon?limit=107&offset=386",
                "https://pokeapi.co/api/v2/pokemon?limit=156&offset=493",
                "https://pokeapi.co/api/v2/pokemon?limit=72&offset=649",
                "https://pokeapi.co/api/v2/pokemon?limit=88&offset=721",
                "https://pokeapi.co/api/v2/pokemon?limit=89&offset=809"
            };

            // creates a new button for each generation
            for (int i = 0; i < urls.Length; i++)
            {
                ComboBoxItem newGen = new ComboBoxItem();
                newGen.Content = "Gen" + (i + 1);
                newGen.Tag = urls[i].ToString();
                newGen.PreviewMouseLeftButtonDown += Fetch;
                gen_pick.Items.Add(newGen);
            }
        }

        public void Fetch(object sender, RoutedEventArgs e)
        {
            cb.Items.Clear();
            ComboBoxItem a = (ComboBoxItem)sender;

            // url for fetching selected generation pokemon from API
            string url = a.Tag.ToString();

            // starts a webclient
            using (WebClient client = new WebClient())
            {
                // downloads the json into a raw string format
                string myJsonResponse = client.DownloadString(url);

                // uses the decoder on the string and puts it in a list
                Pokemon.Root myDeserializedClass = JsonConvert.DeserializeObject<Pokemon.Root>(myJsonResponse);

                for (int i = 0; i < myDeserializedClass.results.Count; i++)
                {
                    // sets the first char of the string to uppercase
                    string numberOfPokemon = myDeserializedClass.results[i].url.Substring(34);
                    string str = TitleCase(myDeserializedClass.results[i].name);
                    ComboBoxItem newBtn = new ComboBoxItem();
                    newBtn.Content = $"{numberOfPokemon.Remove(numberOfPokemon.Length - 1)}" + ": " + str;
                    newBtn.Tag = myDeserializedClass.results[i].url;
                    newBtn.PreviewMouseLeftButtonDown += API_Helper;
                    cb.Items.Add(newBtn);
                }
            }
        }


        // this is an on_click event but works as a load-in method
        private void API_Helper(object sender, RoutedEventArgs e)
        {
            next.IsEnabled = true;
            previous.IsEnabled = true;

            // textbox clear
            tb.Clear();
            ComboBoxItem b = (ComboBoxItem)sender;

            // this takes the Tag from sender, which is pre-set to a url, and converts it to string
            string site = b.Tag.ToString();
            Console.WriteLine(site.ToString());

            PokemonContent(site);
            
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            next.IsEnabled = true;
            previous.IsEnabled = true;
            tb.Clear();
            
            if (searchInput.Text.ToLower().Trim() == null || searchInput.Text.ToLower().Trim() == "")
            {
                searchInput.Text = "Please enter number or name";
            }
            else
            {
                string url = "https://pokeapi.co/api/v2/pokemon/" + searchInput.Text.ToLower().Trim().ToString();
                PokemonContent(url);
            }
        }

        private void PokemonContent(string site)
        {
            // starts a new WebClient
            using (WebClient client = new WebClient())
            {

                // downloads a string using the web url from site
                string myJsonResponse = client.DownloadString(site);

                // uses Newtonsoft to decode the JSON and uses the SelectedPokemon class to populate the lists we're using later
                SelectedPokemon.Root myDeserializedClass = JsonConvert.DeserializeObject<SelectedPokemon.Root>(myJsonResponse);
                thisPokemon = myDeserializedClass;

                // this is what gets written in the textbox refering to variables in the class list
                tb.Text += $"Name: {TitleCase(myDeserializedClass.name)}\n";
                for (int i = 0; i < myDeserializedClass.stats.Count; i++)
                {
                    tb.Text += $"{TitleCase(myDeserializedClass.stats[i].stat.name)}: " +
                    $"{myDeserializedClass.stats[i].base_stat}\n";
                }
                for (int i = 0; i < myDeserializedClass.types.Count; i++)
                {
                    tb.Text += $"Type {i + 1}: {TitleCase(myDeserializedClass.types[i].type.name)}\n";
                }

                // this sets a string with the url from the SelectedPokemon class
                // the API sends different sprites urls and the one we're using is the front_default
                string url = myDeserializedClass.sprites.other.OfficialArtwork.front_default.ToString();

                // creates a new BitmapImage
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(url);
                image.EndInit();

                // icon is an image in the MainWindow GUI
                // the reason we don't set url straight down here is ->
                // because .Source can't take strings
                // instead we can initialize the BitmapImage and use the url there
                icon.Source = image;
            }
        }

        private void Next_Sprite_Click(object sender, RoutedEventArgs e)
        {
            if (index >= 4)
            {
                index = 0;
            }
            else
            {
                index++;
            }
            
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(Sprite(index));
            image.EndInit();
            icon.Source = image;

        }

        private void Previous_Sprite_Click(object sender, RoutedEventArgs e)
        {
            if (index <= 0)
            {
                index = 4;
            }
            else
            {
                index--;
            }
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(Sprite(index));
            image.EndInit();
            icon.Source = image;
        }


        // string array used for cycling through sprites
        // takes an index as argument which is set at the very top
        private string Sprite(int i)
        {
            string[] sprites = {
                thisPokemon.sprites.other.OfficialArtwork.front_default,
                thisPokemon.sprites.front_default,
                thisPokemon.sprites.back_default,
                thisPokemon.sprites.front_shiny,
                thisPokemon.sprites.back_shiny,
            };

            return sprites[i];
        }
    }
}
