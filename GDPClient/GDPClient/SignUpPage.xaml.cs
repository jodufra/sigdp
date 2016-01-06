﻿using Entities;
using GDPClient.Others;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Certificates;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GDPClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SignUpPage : Page
    {
        private IReadOnlyList<Certificate> certList;
        private static string NOCERTS = "No certificates found";
        private Certificate selectedCertificate = null;

        public SignUpPage()
        {
            this.InitializeComponent();
            loadCertificates();
        }

        private async void okBtn_Click(object sender, RoutedEventArgs e)
        {
            List<String> errors = new List<string>();

            var model = new
            {
                register = new
                {
                    username = usernameBox.Text,
                    password = passwordBox.Password,
                    passwordConfirm = passwordConfirmBox.Password,
                    certSubject = selectedCertificate.Subject,
                    certIssuer = selectedCertificate.Issuer,
                    certThumbprint = CryptographicBuffer.EncodeToHexString(CryptographicBuffer.CreateFromByteArray(selectedCertificate.GetHashValue())),
                    certSerialNumber = CryptographicBuffer.EncodeToHexString(CryptographicBuffer.CreateFromByteArray(selectedCertificate.SerialNumber)),
                    certValidFrom = selectedCertificate.ValidFrom.DateTime,
                    certValidTo = selectedCertificate.ValidFrom.DateTime
                }
            };

            if (String.IsNullOrEmpty(model.register.username))
                errors.Add("Username is required");

            if (String.IsNullOrEmpty(model.register.password))
                errors.Add("Password is required");
            else if (model.register.password != passwordConfirmBox.Password)
                errors.Add("Passwords are not equal");

            if (!errors.Any())
            {
                try
                {
                    var url = App.LocalSettings.Values[App.ServiceConn].ToString() + "auth/register";
                    HttpResponseMessage mReceived = await ApiRequest.MakeRequest(url, HttpMethod.Post, model.ToQueryString());
                    errors.Add(JsonConvert.DeserializeObject<String>(await mReceived.Content.ReadAsStringAsync()));
                    mReceived.Dispose();
                }
                catch
                {
                    errors.Add("Unable to request the service!");
                }   
            }
            
            if (errors.Any())
                ShowErrorMsg(String.Join(Environment.NewLine, errors));
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            //Go back to login page
            Frame.Navigate(typeof(LoginPage));
        }

        private void loadCertificates()
        {
            certificatesListCB.Items.Clear();
            var task = CertificateStores.FindAllAsync();
            task.AsTask().Wait();
            var certlist = task.GetResults();

            LoadCertList(certlist);

            if (certificatesListCB.Items.Count == 0)
            {
                certificatesListCB.Items.Add(NOCERTS);
                certificatesListCB.IsEnabled = false;
            }
            else
            {
                certificatesListCB.IsEnabled = true;
            }
            certificatesListCB.SelectedIndex = 0;
        }

        public void LoadCertList(IReadOnlyList<Certificate> certificateList)
        {
            this.certList = certificateList;
            this.certificatesListCB.Items.Clear();

            if (certList.Count > 0)
                certificatesListCB.Items.Add("");
            for (int i = 0; i < certList.Count; i++)
            {
                this.certificatesListCB.Items.Add(certList[i].Subject);
            }
        }

        private void refreshCertsBtn_Click(object sender, RoutedEventArgs e)
        {
            loadCertificates();
        }

        private void certificatesListCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (certificatesListCB.SelectedIndex == 0)
            {
                certDataGrid.Visibility = Visibility.Collapsed;
                selectedCertificate = null;
            }
            else if (certificatesListCB.SelectedIndex > 0)
            {
                //get cert info
                selectedCertificate = certList[certificatesListCB.SelectedIndex - 1];

                issuedToTb.Text = " " + selectedCertificate.Subject;
                issuedByTb.Text = " " + selectedCertificate.Issuer;
                //CryptographicBuffer.EncodeToHexString(CryptographicBuffer.CreateFromByteArray(selectedcertificate.GetHashValue()));
                //CryptographicBuffer.EncodeToHexString(CryptographicBuffer.CreateFromByteArray(selectedcertificate.SerialNumber));
                dateCertFromTb.Text = " " + selectedCertificate.ValidFrom.ToString("dd/MM/yy H:mm:ss");
                dateCertToTb.Text = " " + selectedCertificate.ValidTo.ToString("dd/MM/yy H:mm:ss");
                certDataGrid.Visibility = Visibility.Visible;
            }
        }

        private async void ShowErrorMsg(String msg)
        {
            var messageDialog = new MessageDialog(msg);
            messageDialog.Commands.Add(new UICommand("OK"));
            messageDialog.DefaultCommandIndex = 0;
            await messageDialog.ShowAsync();
        }
    }
}
