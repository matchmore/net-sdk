using System;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Support.Design.Widget;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp.Droid
{
    [Activity(Label = "AddItemActivity")]
    public class AddItemActivity : Activity
    {
        FloatingActionButton saveButton;
        EditText title, description;

        public ItemsViewModel ViewModel { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ViewModel = BrowseFragment.ViewModel;

            // Create your application here
            SetContentView(Resource.Layout.activity_add_item);
            saveButton = FindViewById<FloatingActionButton>(Resource.Id.save_button);
            title = FindViewById<EditText>(Resource.Id.txtTitle);
            description = FindViewById<EditText>(Resource.Id.txtDesc);

            saveButton.Click += SaveButton_Click;
        }

        void SaveButton_Click(object sender, EventArgs e)
        {
            Matchmore.SDK.Matchmore.Configure(new Matchmore.SDK.Config
            {
                ApiKey = "eyJ0eXAiOiJKV1QiLCJhbGciOiJFUzI1NiJ9.eyJpc3MiOiJhbHBzIiwic3ViIjoiZDFhMDhkMjUtOGNjNi00ZjhhLWFlZjAtYjNiNjc5MDE2MjFmIiwiYXVkIjpbIlB1YmxpYyJdLCJuYmYiOjE1MjU3MDI3ODksImlhdCI6MTUyNTcwMjc4OSwianRpIjoiMSJ9.ht7KJrXGXkh8xqC9cFYAJV7NS0kSti3YidUB2nTyeHm7REsIhNKlwuDyfxSkeQZE6o0OHWegn7hZcHoAvW5QOw",
                Environment = "130.211.39.172"
            });

			var r = Task.Run(async () =>
			{
				return await Matchmore.SDK.Matchmore.Instance.SetupMainDevice();
			});

            

			Task.WaitAll(r);

			var d= r.Result;

            var item = new Item
            {
                Text = title.Text,
                Description = description.Text
            };
            ViewModel.AddItemCommand.Execute(item);

            Finish();
        }
    }
}
