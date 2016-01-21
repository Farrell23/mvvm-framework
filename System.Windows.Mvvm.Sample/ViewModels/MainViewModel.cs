﻿
#region Using Directives

using ReactiveUI;
using System.Windows.Mvvm.Reactive;
using System.Windows.Mvvm.Sample.Repositories;
using System.Threading.Tasks;
using System.Windows.Mvvm.Services.Navigation;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using System.Windows.Mvvm.Sample.Views;
using System.Windows.Mvvm.Sample.Models;

#endregion

namespace System.Windows.Mvvm.Sample.ViewModels
{
    /// <summary>
    /// Represents the view model for the main view of the application.
    /// </summary>
    public class MainViewModel : ReactiveViewModel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="MainViewModel"/> instance.
        /// </summary>
        /// <param name="todoListItemsRepository">The todo list items repository, which can be used to manage the items on the todo list.</param>
        public MainViewModel(NavigationService navigationService, TodoListItemsRepository todoListItemsRepository)
        {
            this.navigationService = navigationService;
            this.todoListItemsRepository = todoListItemsRepository;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the navigation service, which is used to navigate between views.
        /// </summary>
        private readonly NavigationService navigationService;

        /// <summary>
        /// Contains the todo list items repository, which can be used to manage the items on the todo list.
        /// </summary>
        private readonly TodoListItemsRepository todoListItemsRepository;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the items of the todo list.
        /// </summary>
        public ReactiveList<TodoListItemViewModel> TodoListItems { get; private set; } = new ReactiveList<TodoListItemViewModel> { ChangeTrackingEnabled = true };

        /// <summary>
        /// Contains the currently selected todo list item.
        /// </summary>
        private TodoListItemViewModel selectedTodoListItem;

        /// <summary>
        /// Gets or sets the currently selected todo list item.
        /// </summary>
        public TodoListItemViewModel SelectedTodoListItem
        {
            get
            {
                return this.selectedTodoListItem;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedTodoListItem, value);
            }
        }

        /// <summary>
        /// Gets the command, which marks the currently selected todo list item as finished.
        /// </summary>
        public ReactiveCommand<Unit> MarkTodoListItemAsFinishedCommand { get; private set; }

        /// <summary>
        /// Gets the command, which removes the currently selected item from the todo list.
        /// </summary>
        public ReactiveCommand<Unit> RemoveTodoListItemCommand { get; private set; }

        /// <summary>
        /// Gets the command, which navigates the user to the create todo list item view.
        /// </summary>
        public ReactiveCommand<NavigationResult> CreateTodoListItemCommand { get; private set; }

        #endregion

        #region ReactiveViewModel Implementation

        /// <summary>
        /// Is called when the view model is created. Initializes the commands of the view model.
        /// </summary>
        public override Task OnActivateAsync()
        {
            // Initializes the command, which marks the currently selected todo list item as finished
            this.MarkTodoListItemAsFinishedCommand = ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.SelectedTodoListItem).Select(x => this.SelectedTodoListItem != null), x =>
            {
                this.SelectedTodoListItem.IsFinished = true;
                this.todoListItemsRepository.MarkTodoListItemAsFinished(this.SelectedTodoListItem.Id);
                return Task.FromResult(Unit.Default);
            });

            // Initializes the command, which removes the currently selected todo list item
            this.RemoveTodoListItemCommand = ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.SelectedTodoListItem).Select(x => this.SelectedTodoListItem != null), x =>
            {
                this.TodoListItems.Remove(this.SelectedTodoListItem);
                this.todoListItemsRepository.RemoveTodoListItem(this.SelectedTodoListItem.Id);
                this.SelectedTodoListItem = null;
                return Task.FromResult(Unit.Default);
            });

            // Initializes the command, which navigates the user to the create todo list item view
            this.CreateTodoListItemCommand = ReactiveCommand.CreateAsyncTask(async x =>  await this.navigationService.NavigateAsync<CreateTodoListItemView>());

            // Since no asynchronous operation was performed, an empty task is returned
            return Task.FromResult(0);
        }

        /// <summary>
        /// Is called when the user is navigated to the view of this view model. Loads the todo list items from storage.
        /// </summary>
        /// <param name="e">The navigation arguments, that contain more information about the navigation.</param>
        public override Task OnNavigateToAsync(NavigationEventArgs e)
        {
            // Clears the current list of todo list items first
            this.TodoListItems.Clear();

            // Loads all the todo list items from the repository and stores them in a list, so that the view has access to them
            foreach (TodoListItem todoListItem in this.todoListItemsRepository.GetTodoListItems().ToList())
            {
                this.TodoListItems.Add(new TodoListItemViewModel
                {
                    Id = todoListItem.Id,
                    Title = todoListItem.Title,
                    Description = todoListItem.Description,
                    IsFinished = todoListItem.IsFinished
                });
            }

            // Since no asynchronous operation was performed, an empty task is returned
            return Task.FromResult(0);
        }

        #endregion
    }
}