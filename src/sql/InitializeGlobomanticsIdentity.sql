USE master
GO
DROP DATABASE IF EXISTS GlobomanticsIdentity
GO

CREATE DATABASE GlobomanticsIdentity
GO 
USE GlobomanticsIdentity
GO 

CREATE TABLE [dbo].[TestLog](
	[Id] [INT] IDENTITY(1,1) NOT NULL,
	[InfoMessage] [NVARCHAR](MAX) NULL,
	[MessageAtDate] [DATETIME] NULL,
 CONSTRAINT [PK_TestLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

INSERT INTO [dbo].[TestLog] (InfoMessage, MessageAtDate) VALUES ('Database crated', GETDATE() )
