using System;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;

namespace Client
{
	class Program
	{
		static void Main(string[] args)
		{
			HttpClient client = new HttpClient();
			client.Timeout = new TimeSpan(0, 20, 0); // 20 minutter til store downloads
			client.BaseAddress = new Uri("http://dawa.aws.dk/");

			AutocompleteVejnavne(client, "q=rødkildevej");
			AutocompleteAdresser(client, "q=rødkildevej");
			GetAdresse(client, "0a3f50a0-73bf-32b8-e044-0003ba298018");
			GetAdresser(client, "vejnavn=Rødkildevej&postnr=2400");
			GetAdresserStreaming(client, "kommunekode=0101");

			Console.ReadLine();
		}


		static void AutocompleteVejnavne(HttpClient client, string query)
		{
			try
			{
				string url = "vejnavne/autocomplete" + (query.Length == 0 ? "" : "?") + query;
				Console.WriteLine("GET " + url);
				HttpResponseMessage response = client.GetAsync(url).Result;
				response.EnsureSuccessStatusCode();
				string responseBody = response.Content.ReadAsStringAsync().Result;
				dynamic vejnavne = JArray.Parse(responseBody);

				int antal = 0;
				foreach (dynamic vejnavn in vejnavne)
				{
					Console.WriteLine(formatVejnavneAutocomplete(vejnavn));
					antal++;
				}
				Console.WriteLine("{0} adresser", antal);
			}
			catch (HttpRequestException e)
			{
				Console.WriteLine("Message :{0} ", e.Message);
			}
		}


		static void AutocompleteAdresser(HttpClient client, string query)
		{
			try
			{
				string url = "adresser/autocomplete" + (query.Length == 0 ? "" : "?") + query;
				Console.WriteLine("GET " + url);
				HttpResponseMessage response = client.GetAsync(url).Result;
				response.EnsureSuccessStatusCode();
				string responseBody = response.Content.ReadAsStringAsync().Result;
				dynamic adresser = JArray.Parse(responseBody);

				int antal = 0;
				foreach (dynamic adresse in adresser)
				{
					Console.WriteLine(formatAdresseAutocomplete(adresse));
					antal++;
				}
				Console.WriteLine("{0} adresser", antal);
			}
			catch (HttpRequestException e)
			{
				Console.WriteLine("Message :{0} ", e.Message);
			}
		}

		static void GetAdresse(HttpClient client, string id)
		{
			try
			{
				string url = "adresser/" + id;
				Console.WriteLine("GET " + url);
				HttpResponseMessage response = client.GetAsync(url).Result;
				response.EnsureSuccessStatusCode();
				string responseBody = response.Content.ReadAsStringAsync().Result;
				dynamic adresse = JValue.Parse(responseBody);
				Console.WriteLine(formatAdresse(adresse));
			}
			catch (HttpRequestException e)
			{
				Console.WriteLine("Message :{0} ", e.Message);
			}
		}

		static void GetAdresser(HttpClient client, string query)
		{
			try
			{
				string url = "adresser" + (query.Length == 0 ? "" : "?") + query;
				Console.WriteLine("GET " + url);
				HttpResponseMessage response = client.GetAsync(url).Result;
				response.EnsureSuccessStatusCode();
				string responseBody = response.Content.ReadAsStringAsync().Result;
				dynamic adresser = JArray.Parse(responseBody);

				int antal = 0;
				foreach (dynamic adresse in adresser)
				{
					Console.WriteLine(formatAdresse(adresse));
					antal++;
				}
				Console.WriteLine("{0} adresser", antal);
			}
			catch (HttpRequestException e)
			{
				Console.WriteLine("Message :{0} ", e.Message);
			}
		}

		static void GetAdresserStreaming(HttpClient client, string query)
		{
			try
			{
				string url = "adresser" + (query.Length == 0 ? "" : "?") + query;
				Console.WriteLine("GET " + url);
				var stream = client.GetStreamAsync(url).Result;
				stream.ReadTimeout = 20 * 60 * 60 * 1000; //20 minutter
				var streamReader = new StreamReader(stream);
				JsonTextReader reader = new JsonTextReader(streamReader);

				int antal = 0;
				using (reader)
				{
					while (reader.Read())
					{
						if (reader.TokenType == JsonToken.StartObject)
						{
							dynamic adresse = JObject.Load(reader);
							Console.WriteLine("{0}: {1}", antal.ToString(), formatAdresse(adresse));
							antal++;
						}
					}
				}
				Console.WriteLine("{0} adresser", antal);

			}
			catch (HttpRequestException e)
			{
				Console.WriteLine("Message :{0} ", e.Message);
			}
		}
		
		static string formatAdresse(dynamic adresse)
		{
			return string.Format("{0} {1} {2} {3}, {4} {5}", adresse.adgangsadresse.vejstykke.navn, adresse.adgangsadresse.husnr, adresse.etage, adresse.dør, adresse.adgangsadresse.postnummer.nr, adresse.adgangsadresse.postnummer.navn);
		}

		static string formatVejnavneAutocomplete(dynamic av)
		{
			return string.Format("{0}: {1}", av.tekst, av.vejnavn.href);
		}

		static string formatAdresseAutocomplete(dynamic a)
		{
			return string.Format("{0}: {1}", a.tekst, a.adresse.href);
		}
	}
}
