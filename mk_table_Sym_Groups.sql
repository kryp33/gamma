USE [Syms]
GO

/****** Object:  Table [dbo].[Groups]    Script Date: 07/03/2014 16:06:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Groups](
	[groupName] [varchar](50) NOT NULL,
	[groupNotes] [varchar](max) NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

