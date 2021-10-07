/****** BlogEngine.NET 1.3 SQL Setup Script ******/
/****** Object:  Table [dbo].[be_Categories]    Script Date: 12/22/2007 14:14:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[be_Categories](
	[CategoryID] [uniqueidentifier] ROWGUIDCOL  NOT NULL CONSTRAINT [DF_be_Categories_CategoryID]  DEFAULT (newid()),
	[CategoryName] [nvarchar](50) NULL,
	[Description] [nvarchar](200) NULL,
 CONSTRAINT [PK_be_Categories] PRIMARY KEY CLUSTERED 
(
	[CategoryID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[be_Pages]    Script Date: 12/22/2007 14:15:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[be_Pages](
	[PageID] [uniqueidentifier] ROWGUIDCOL  NOT NULL CONSTRAINT [DF_be_Pages_PageID]  DEFAULT (newid()),
	[Title] [nvarchar](255) NULL,
	[Description] [nvarchar](max) NULL,
	[PageContent] [ntext] NULL,
	[Keywords] [nvarchar](max) NULL,
	[DateCreated] [datetime] NULL,
	[DateModified] [datetime] NULL,
	[IsPublished] [bit] NULL,
	[IsFrontPage] [bit] NULL,
	[Parent] [uniqueidentifier] NULL,
	[ShowInList] [bit] NULL,
 CONSTRAINT [PK_be_Pages] PRIMARY KEY CLUSTERED 
(
	[PageID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[be_PingService]    Script Date: 12/22/2007 14:15:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[be_PingService](
	[PingServiceID] [int] IDENTITY(1,1) NOT NULL,
	[Link] [nvarchar](255) NULL,
 CONSTRAINT [PK_be_PingService] PRIMARY KEY CLUSTERED 
(
	[PingServiceID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[be_Posts]    Script Date: 12/22/2007 14:16:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[be_Posts](
	[PostID] [uniqueidentifier] ROWGUIDCOL  NOT NULL CONSTRAINT [DF_be_Posts_PostID]  DEFAULT (newid()),
	[Title] [nvarchar](255) NULL,
	[Description] [nvarchar](max) NULL,
	[PostContent] [ntext] NULL,
	[DateCreated] [datetime] NULL,
	[DateModified] [datetime] NULL,
	[Author] [nvarchar](50) NULL,
	[IsPublished] [bit] NULL,
	[IsCommentEnabled] [bit] NULL,
	[Raters] [int] NULL,
	[Rating] [real] NULL,
	[Slug] [nvarchar](255) NULL,
 CONSTRAINT [PK_be_Posts] PRIMARY KEY CLUSTERED 
(
	[PostID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[be_Settings]    Script Date: 12/22/2007 14:16:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[be_Settings](
	[SettingName] [nvarchar](50) NOT NULL,
	[SettingValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_be_Settings] PRIMARY KEY CLUSTERED 
(
	[SettingName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[be_PostCategory]    Script Date: 12/22/2007 14:17:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[be_PostCategory](
	[PostCategoryID] [int] IDENTITY(1,1) NOT NULL,
	[PostID] [uniqueidentifier] NOT NULL,
	[CategoryID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_be_PostCategory] PRIMARY KEY CLUSTERED 
(
	[PostCategoryID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[be_PostCategory]  WITH CHECK ADD  CONSTRAINT [FK_be_PostCategory_be_Categories] FOREIGN KEY([CategoryID])
REFERENCES [dbo].[be_Categories] ([CategoryID])
GO
ALTER TABLE [dbo].[be_PostCategory] CHECK CONSTRAINT [FK_be_PostCategory_be_Categories]
GO
ALTER TABLE [dbo].[be_PostCategory]  WITH CHECK ADD  CONSTRAINT [FK_be_PostCategory_be_Posts] FOREIGN KEY([PostID])
REFERENCES [dbo].[be_Posts] ([PostID])
GO
ALTER TABLE [dbo].[be_PostCategory] CHECK CONSTRAINT [FK_be_PostCategory_be_Posts]
GO
/****** Object:  Table [dbo].[be_PostComment]    Script Date: 12/22/2007 14:17:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[be_PostComment](
	[PostCommentID] [uniqueidentifier] ROWGUIDCOL  NOT NULL CONSTRAINT [DF_be_PostComment_PostCommentID]  DEFAULT (newid()),
	[PostID] [uniqueidentifier] NOT NULL,
	[CommentDate] [datetime] NOT NULL,
	[Author] [nvarchar](255) NULL,
	[Email] [nvarchar](255) NULL,
	[Website] [nvarchar](255) NULL,
	[Comment] [nvarchar](max) NULL,
	[Country] [nvarchar](255) NULL,
	[Ip] [nvarchar](50) NULL,
	[IsApproved] [bit] NULL,
 CONSTRAINT [PK_be_PostComment] PRIMARY KEY CLUSTERED 
(
	[PostCommentID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[be_PostComment]  WITH CHECK ADD  CONSTRAINT [FK_be_PostComment_be_Posts] FOREIGN KEY([PostID])
REFERENCES [dbo].[be_Posts] ([PostID])
GO
ALTER TABLE [dbo].[be_PostComment] CHECK CONSTRAINT [FK_be_PostComment_be_Posts]
GO
/****** Object:  Table [dbo].[be_PostNotify]    Script Date: 12/22/2007 14:17:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[be_PostNotify](
	[PostNotifyID] [int] IDENTITY(1,1) NOT NULL,
	[PostID] [uniqueidentifier] NOT NULL,
	[NotifyAddress] [nvarchar](255) NULL,
 CONSTRAINT [PK_be_PostNotify] PRIMARY KEY CLUSTERED 
(
	[PostNotifyID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[be_PostNotify]  WITH CHECK ADD  CONSTRAINT [FK_be_PostNotify_be_Posts] FOREIGN KEY([PostID])
REFERENCES [dbo].[be_Posts] ([PostID])
GO
ALTER TABLE [dbo].[be_PostNotify] CHECK CONSTRAINT [FK_be_PostNotify_be_Posts]
GO
/****** Object:  Table [dbo].[be_PostTag]    Script Date: 12/22/2007 14:17:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[be_PostTag](
	[PostTagID] [int] IDENTITY(1,1) NOT NULL,
	[PostID] [uniqueidentifier] NOT NULL,
	[Tag] [nvarchar](50) NULL,
 CONSTRAINT [PK_be_PostTag] PRIMARY KEY CLUSTERED 
(
	[PostTagID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[be_PostTag]  WITH CHECK ADD  CONSTRAINT [FK_be_PostTag_be_Posts] FOREIGN KEY([PostID])
REFERENCES [dbo].[be_Posts] ([PostID])
GO
ALTER TABLE [dbo].[be_PostTag] CHECK CONSTRAINT [FK_be_PostTag_be_Posts]
GO
/****** Object:  Index [FK_PostID]    Script Date: 12/22/2007 14:18:36 ******/
CREATE NONCLUSTERED INDEX [FK_PostID] ON [dbo].[be_PostCategory] 
(
	[PostID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Index [FK_CategoryID]    Script Date: 12/22/2007 14:19:19 ******/
CREATE NONCLUSTERED INDEX [FK_CategoryID] ON [dbo].[be_PostCategory] 
(
	[CategoryID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Index [FK_PostID]    Script Date: 12/22/2007 14:19:45 ******/
CREATE NONCLUSTERED INDEX [FK_PostID] ON [dbo].[be_PostComment] 
(
	[PostID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Index [FK_PostID]    Script Date: 12/22/2007 14:20:29 ******/
CREATE NONCLUSTERED INDEX [FK_PostID] ON [dbo].[be_PostNotify] 
(
	[PostID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Index [FK_PostID]    Script Date: 12/22/2007 14:20:43 ******/
CREATE NONCLUSTERED INDEX [FK_PostID] ON [dbo].[be_PostTag] 
(
	[PostID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

/***  Load initial Data ***/
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('alternatefeedurl', '');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('authorname', 'My name');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('avatar', 'combine');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('blogrollmaxlength', '23');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('blogrollupdateminutes', '60');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('blogrollvisibleposts', '3');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('contactformmessage', '<p>I will answer the mail as soon as I can.</p>');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('contactthankmessage', '<h1>Thank you</h1><p>The message was sent.</p>');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('culture', 'Auto');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('dayscommentsareenabled', '0');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('description', 'Short description of the blog');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('displaycommentsonrecentposts', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('displayratingsonrecentposts', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('email', 'user@example.com');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('emailsubjectprefix', 'Weblog');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('enablecommentsearch', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('enablecommentsmoderation', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('enablecontactattachments', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('enablecountryincomments', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('enablehttpcompression', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('enableopensearch', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('enablepingbackreceive', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('enablepingbacksend', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('enablerating', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('enablereferrertracking', 'False');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('enablerelatedposts', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('enablessl', 'False');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('enabletrackbackreceive', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('enabletrackbacksend', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('endorsement', 'http://www.dotnetblogengine.net/syndication.axd');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('fileextension', '.aspx');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('geocodinglatitude', '0');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('geocodinglongitude', '0');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('handlewwwsubdomain', '');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('htmlheader', '');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('iscocommentenabled', 'False');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('iscommentsenabled', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('language', 'en-GB');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('name', 'Name of the blog');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('numberofrecentcomments', '10');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('numberofrecentposts', '10');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('postsperfeed', '15');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('postsperpage', '10');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('removewhitespaceinstylesheets', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('searchbuttontext', 'Search');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('searchcommentlabeltext', 'Include comments in search');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('searchdefaulttext', 'Enter search term');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('sendmailoncomment', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('showdescriptioninpostlist', 'False');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('showlivepreview', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('showpostnavigation', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('smtppassword', 'password');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('smtpserver', 'mail.example.dk');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('smtpserverport', '25');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('smtpusername', 'user@example.com');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('storagelocation', '~/App_Data/');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('syndicationformat', 'Rss');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('theme', 'Standard');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('timestamppostlinks', 'True');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('timezone', '1');
INSERT INTO be_Settings (SettingName, SettingValue)	VALUES ('trackingscript', '');

DECLARE @postID uniqueidentifier, @catID uniqueidentifier

SET @postID = NEWID();
SET @catID = NEWID();

INSERT INTO be_Categories (CategoryID, CategoryName)
	VALUES (@catID, 'General');

INSERT INTO be_Posts (PostID, Title, Description, PostContent, DateCreated, Author, IsPublished)
	VALUES (@postID, 
	'Welcome to BlogEngine.NET 1.3 with MSSQL provider', 
	'The description is used as the meta description as well as shown in the related posts. It is recommended that you write a description, but not mandatory',
	'<p>If you see this post it means that BlogEngine.NET 1.3 is running and the SQL Server provider is configured correctly.</p>
	<h2>Setup</h2>
	<p>If you are using the ASP.NET Membership provider, you are set to use existing users.  If you are using the default BlogEngine.NET XML provider, it is time to setup some users.  Find the sign-in link located either at the bottom or top of the page depending on your current theme and click it. Now enter "admin" in both the username and password fields and click the button. You will now see an admin menu appear. It has a link to the "Users" admin page. From there you can change the username and password.</p>
	<h2>Write permissions</h2>
	<p>Since you are using SQL to store your posts, most information is stored there.  However, if you want to store attachments or images in the blog, you will want write permissions setup on the App_Data folder.</p>
	<h2>On the web </h2>
	<p>You can find BlogEngine.NET on the <a href="http://www.dotnetblogengine.net">official website</a>. Here you will find tutorials, documentation, tips and tricks and much more. The ongoing development of BlogEngine.NET can be followed at <a href="http://www.codeplex.com/blogengine">CodePlex</a> where the daily builds will be published for anyone to download.</p>
	<p>Good luck and happy writing.</p>
	<p>The BlogEngine.NET team</p>',
	'09/30/07', 
	'admin',
	1);

INSERT INTO be_PostCategory (PostID, CategoryID)
	VALUES (@postID, @catID);

INSERT INTO be_PostTag (PostID, Tag)
	VALUES (@postID, 'blog');
INSERT INTO be_PostTag (PostID, Tag)
	VALUES (@postID, 'welcome');