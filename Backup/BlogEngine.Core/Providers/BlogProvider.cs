#region Using

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using BlogEngine.Core;

#endregion

namespace BlogEngine.Core.Providers
{
    /// <summary>
    /// A base class for all custom providers to inherit from.
    /// </summary>
    public abstract class BlogProvider : ProviderBase
    {
        // Post
        /// <summary>
        /// Retrieves a Post from the provider based on the specified id.
        /// </summary>
        public abstract Post SelectPost(Guid id);
        /// <summary>
        /// Inserts a new Post into the data store specified by the provider.
        /// </summary>
        public abstract void InsertPost(Post post);
        /// <summary>
        /// Updates an existing Post in the data store specified by the provider.
        /// </summary>
        public abstract void UpdatePost(Post post);
        /// <summary>
        /// Deletes a Post from the data store specified by the provider.
        /// </summary>
        public abstract void DeletePost(Post post);
        /// <summary>
        /// Retrieves all Posts from the provider and returns them in a List.
        /// </summary>
        public abstract List<Post> FillPosts();

        // Page
        /// <summary>
        /// Retrieves a Page from the provider based on the specified id.
        /// </summary>
        public abstract Page SelectPage(Guid id);
        /// <summary>
        /// Inserts a new Page into the data store specified by the provider.
        /// </summary>
        public abstract void InsertPage(Page page);
        /// <summary>
        /// Updates an existing Page in the data store specified by the provider.
        /// </summary>
        public abstract void UpdatePage(Page page);
        /// <summary>
        /// Deletes a Page from the data store specified by the provider.
        /// </summary>
        public abstract void DeletePage(Page page);
        /// <summary>
        /// Retrieves all Pages from the provider and returns them in a List.
        /// </summary>
        public abstract List<Page> FillPages();

       /// <summary>
        /// Retrieves a Category from the provider based on the specified id.
        /// </summary>
        public abstract Category SelectCategory(Guid id);
        /// <summary>
        /// Inserts a new Category into the data store specified by the provider.
        /// </summary>
        public abstract void InsertCategory(Category category);
        /// <summary>
        /// Updates an existing Category in the data store specified by the provider.
        /// </summary>
        public abstract void UpdateCategory(Category category);
        /// <summary>
        /// Deletes a Category from the data store specified by the provider.
        /// </summary>
        public abstract void DeleteCategory(Category category);
        /// <summary>
        /// Retrieves all Categories from the provider and returns them in a List.
        /// </summary>
        public abstract List<Category> FillCategories();

        // Settings
        /// <summary>
        /// Loads the settings from the provider.
        /// </summary>
        public abstract StringDictionary LoadSettings();
        /// <summary>
        /// Saves the settings to the provider.
        /// </summary>
        public abstract void SaveSettings(StringDictionary settings);

        //Ping services
        /// <summary>
        /// Loads the ping services.
        /// </summary>
        /// <returns></returns>
        public abstract StringCollection LoadPingServices();
        /// <summary>
        /// Saves the ping services.
        /// </summary>
        /// <param name="services">The services.</param>
        public abstract void SavePingServices(StringCollection services);

    }

    /// <summary>
    /// A collection of all registered providers.
    /// </summary>
    public class BlogProviderCollection : ProviderCollection
    {
        /// <summary>
        /// Gets a provider by its name.
        /// </summary>
        public new BlogProvider this[string name]
        {
            get { return (BlogProvider)base[name]; }
        }

        /// <summary>
        /// Add a provider to the collection.
        /// </summary>
        public override void Add(ProviderBase provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (!(provider is BlogProvider))
                throw new ArgumentException
                    ("Invalid provider type", "provider");

            base.Add(provider);
        }
    }

}
