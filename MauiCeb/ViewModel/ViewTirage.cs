//-----------------------------------------------------------------------
// <copyright file="ViewTirage.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#region Using

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;

using arnaud.morin.outils;

using CompteEstBon;

#endregion

// ReSharper disable EnforceIfStatementBraces
namespace MauiCeb.ViewModel {
	public class ViewTirage : INotifyPropertyChanged, ICommand {
		private readonly Stopwatch _notifyWatch = new();

		private bool _auto;
		private Color _background = Color.FromRgb(22, 22, 22);

		private Color _foreground = Colors.White;

		private bool _isBusy;
		
		
		private bool _popup;

		private string _result = "Résoudre";

		private CebBase _solution = null!;

		private string _titre = "Jeu du Compte Est Bon";


		/// <summary>
		///     Initialisation
		/// </summary>
		/// <returns>
		/// </returns>
		public ViewTirage() {
			
			Plaques.CollectionChanged += async (_, e) => {
				if (e.Action != NotifyCollectionChangedAction.Replace || IsBusy)
					return;

				var i = e.NewStartingIndex;
				Tirage.Plaques[i].Value = Plaques[i];
				await ClearAsync();

				if (Auto)
					await ResolveAsync();
			};
			Auto = false;
			
			UpdateData();
			Titre = $"{Result} - {DateTime.Now:dddd dd MMMM yyyy à HH:mm:ss}";
		}

		public static string DotnetVersion =>
			$"Version: {Assembly.GetExecutingAssembly().GetName().Version}, {RuntimeInformation.FrameworkDescription}";

		public Color Background {
			get => _background;
			set {
				_background = value;
				OnPropertyChanged();
			}
		}

		public static IEnumerable<int> ListePlaques => CebPlaque.DistinctPlaques;

		public CebTirage Tirage { get; } = new();

		public ObservableCollection<int> Plaques { get; } = [0, 0, 0, 0, 0, 0];

		public IEnumerable<CebBase> Solutions {
			get => Tirage.Solutions;
			// ReSharper disable once ValueParameterNotUsed
			set => OnPropertyChanged(nameof(Solutions), nameof(Count));
		}
		

		public TimeSpan Duree {
			get => Tirage.Duree;
			// ReSharper disable once ValueParameterNotUsed
			set => OnPropertyChanged();
		}


		

		public CebBase Solution {
			get => _solution;
			set {
				_solution = value;
				OnPropertyChanged();
			}
		}

		public int Search {
			get => Tirage.Search;
			set {
				if (Tirage.Search == value)
					return;

				Tirage.Search = value;
				OnPropertyChanged();
				ClearData();
				if (Auto && Tirage.Status == CebStatus.Valide)
					Task.Run(ResolveAsync);
			}
		}

		public Color Foreground {
			get => _foreground;
			set {
				_foreground = value;
				OnPropertyChanged();
			}
		}

		public string Result {
			get => _result;
			set {
				if (value == _result)
					return;

				_result = value;
				OnPropertyChanged();
			}
		}

		public bool IsBusy {
			get => _isBusy;
			set {
				if (_isBusy == value)
					return;

				_isBusy = value;
				OnPropertyChanged();
			}
		}

		public bool Auto {
			get => _auto;
			set {
				if (_auto == value)
					return;

				_auto = value;
				OnPropertyChanged();
				if (Tirage.Status == CebStatus.Valide && Auto)
					Task.Run(ResolveAsync);
			}
		}

		

		public bool IsComputed {
			get => Tirage.Status is CebStatus.CompteEstBon or CebStatus.CompteApproche;
			// ReSharper disable once ValueParameterNotUsed
			set => OnPropertyChanged();
		}

		public int Count {
			get => Tirage.Count;
			// ReSharper disable once ValueParameterNotUsed
			set => OnPropertyChanged();
		}

		public bool Popup {
			get => _popup;
			set {
				if (_popup == value)
					return;

				_popup = value;
				_notifyWatch.Stop();
				_notifyWatch.Reset();
				if (_popup)
					_notifyWatch.Start();

				OnPropertyChanged();
			}
		}

		public string Titre {
			get => _titre;
			set {
				_titre = value;
				OnPropertyChanged();
			}
		}

		//public event EventHandler CanExecuteChanged {
		//	add => CommandManager.RequerySuggested += value;
		//	remove => CommandManager.RequerySuggested -= value;
		//}

		public bool CanExecute(object? parameter) {
			return true;
		}

		public async void Execute(object? parameter) {
			var cmd = (parameter as string)?.ToLower();
			switch (cmd) {
				case "random":
					await RandomAsync();
					break;

				case "resolve":
					switch (Tirage.Status) {
						case CebStatus.Valide:
							 await ResolveAsync();
							break;

						case CebStatus.CompteEstBon or CebStatus.CompteApproche:
							await ClearAsync();
							break;

						case CebStatus.Invalide:
							await RandomAsync();
							break;
					}

					break;

				case "export":
					if (Count != 0)
						await Task.Run(ExportFichier);

					break;
			}
		}

		public event EventHandler? CanExecuteChanged;

		private void ClearData() {
			Solution = null!;
			UpdateForeground();
			Result = Tirage.Status != CebStatus.Invalide ? "" : "Tirage invalide";
			Popup = false;
			OnPropertyChanged(nameof(IsComputed), nameof(Duree), nameof(Solutions), nameof(Duree));
		}

		private void UpdateData() {
			IsBusy = true;
			lock (Plaques) {
				foreach (var (p, i) in Tirage.Plaques.Indexed()) Plaques[i] = p.Value;
			}
OnPropertyChanged(nameof(Plaques));
			OnPropertyChanged(nameof(Search));
			IsBusy = false;
			ClearData();
		}

		private void UpdateForeground() {
			Foreground =
				Tirage.Status switch {
					CebStatus.Indefini => Colors.Blue,
					CebStatus.Valide => Colors.White,
					CebStatus.EnCours => Colors.Aqua,
					CebStatus.CompteEstBon => Colors.SpringGreen,
					CebStatus.CompteApproche => Colors.Orange,
					CebStatus.Invalide => Colors.Red,
					_ => throw new NotImplementedException()
				};
		}

		public void ShowPopup(int index = 0) {
			if (index < 0)
				return;
			Solution = Tirage.Solutions[index];
			Popup = true;
		}

		public static (bool Ok, string Path) SaveFileName() {
			//var dialog = new SaveFileDialog {
			//	Title = "Exporter vers...",
			//	Filter =
			//		"Excel (*.xlsx)| *.xlsx | Word (*.docx) | *.docx | Json (*.json) | *.json | XML (*.xml) | *.xml ",
			//	InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal),
			//	DefaultExt = ".xlsx",
			//	FileName = "*.xlsx"
			//};
			//// ReSharper disable once PossibleInvalidOperationException
			//return ((bool)dialog.ShowDialog(), dialog.FileName);
			return (false, string.Empty);
		}

		private void ExportFichier() {
			//var (ok, path) = SaveFileName();
			//if (!ok)
			//	return;

			//IsBusy = true;
			//if (Tirage.Export(path))
			//	path.OpenDocument();
			//IsBusy = false;
		}

		#region Action

		public async ValueTask ClearAsync() {
			var old = IsBusy;
			await Tirage.ClearAsync();
			ClearData();
			IsBusy = old;
		}

		public async ValueTask RandomAsync() {
			await Tirage.RandomAsync();
			UpdateData();
			if (Auto)
				 await ResolveAsync();
		}

		public async ValueTask<CebStatus> ResolveAsync() {
			if (IsBusy)
				return Tirage.Status;

			IsBusy = true;
			Result = "⏰ Calcul en cours...";
			Foreground = Colors.Aqua;
await	Tirage.ResolveAsync();
			Result = Tirage.Status switch {
				CebStatus.CompteEstBon => "😊 Compte est Bon",
				CebStatus.CompteApproche => $"😢 Compte approché: {Tirage.Found}, écart: {Tirage.Ecart}",
				CebStatus.Invalide => "🤬 Tirage invalide",
				_ => ""
			};

			Solution = Tirage.Solutions[0];

			UpdateForeground();
			IsBusy = false;
			OnPropertyChanged( nameof(Duree),
				nameof(Solutions), nameof(Count));
			ShowPopup();

			return Tirage.Status;
		}

		#endregion Action

		public event PropertyChangedEventHandler? PropertyChanged;
		

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnPropertyChanged(params string[] properties) {
			foreach (var property in properties) {
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
			}
			
		}

		protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
			if (EqualityComparer<T>.Default.Equals(field, value)) return false;
			field = value;
			OnPropertyChanged(propertyName);
			return true;
		}
	}
}