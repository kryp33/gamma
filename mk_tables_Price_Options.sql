USE [Price]
GO
/****** Object:  Table [dbo].[Options]    Script Date: 07/03/2014 17:20:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Options](
	[symbol] [varchar](128) NOT NULL,
	[tradeDate] [int] NOT NULL,
	[tradeTime] [int] NOT NULL,
	[underlier] [varchar](32) NOT NULL,
	[secType] [varchar](16) NULL,
	[subType] [varchar](32) NULL,
	[multiplier] [int] NOT NULL,
	[expDate] [int] NOT NULL,
	[opType] [varchar](4) NOT NULL,
	[strike] [real] NOT NULL,
	[src] [int] NULL,
	[lastPx] [real] NULL,
	[bid] [real] NULL,
	[ask] [real] NULL,
	[volume] [int] NULL,
	[openInt] [int] NULL,
	[underPx] [real] NULL,
	[impVol] [real] NULL,
	[delta] [real] NULL,
	[gamma] [real] NULL,
	[theta] [real] NULL,
	[rho] [real] NULL,
	[vega] [real] NULL,
 CONSTRAINT [PK_Options] PRIMARY KEY CLUSTERED 
(
	[symbol] ASC,
	[tradeDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[OptionErrors]    Script Date: 07/03/2014 17:20:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[OptionErrors](
	[underSymbol] [varchar](50) NOT NULL,
	[tradeDate] [int] NOT NULL,
	[expYear] [int] NOT NULL,
	[expMonth] [int] NOT NULL,
	[dldAttempt] [int] NOT NULL,
	[lastAttDate] [int] NOT NULL,
	[lastAttTime] [int] NOT NULL,
	[success] [int] NOT NULL,
	[notes] [varchar](50) NULL,
	[groupName] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Table] PRIMARY KEY CLUSTERED 
(
	[underSymbol] ASC,
	[tradeDate] ASC,
	[expYear] ASC,
	[expMonth] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Default [DF__OptionErr__dldAt__35BCFE0A]    Script Date: 07/03/2014 17:20:19 ******/
ALTER TABLE [dbo].[OptionErrors] ADD  DEFAULT ((1)) FOR [dldAttempt]
GO
/****** Object:  Default [DF__OptionErr__succe__36B12243]    Script Date: 07/03/2014 17:20:19 ******/
ALTER TABLE [dbo].[OptionErrors] ADD  DEFAULT ((0)) FOR [success]
GO
