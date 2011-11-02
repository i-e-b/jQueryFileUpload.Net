<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Custom.aspx.cs" Inherits="jQueryUploadTest.Custom" %>
<!DOCTYPE HTML>
<html lang="en" class="no-js">
<head>
<meta charset="utf-8">
<title>jQuery File Upload Example - cleaner markup!</title>
<!--[if lt IE 9]>
<script src="http://html5shim.googlecode.com/svn/trunk/html5.js"></script>
<![endif]-->
</head>
<body>
<div id="fileupload">
    <!-- Would like the cleaner style to look like this: -->
	<div upload-target="here" action="FileTransferHandler.ashx">
		<table>  <!-- can use outer containers -->
			<!-- semantic-style template for... template tags always invisible in css -->
			<template for="error">
				<tr class="ui-state-error">
					<td data="name"></td>    <!-- Data is filled in by attribute under the template -->
					<td data="size"></td>
					<td data="error" colspan="2"></td> <!-- error messages are outside of the template -->
				</tr>
			</template>

			<template for="upload">
				<tr>
					<td data="name"></td>
					<td data="size"></td>
					<td data="progress"></td>
					<td data="cancel"></td>
				</tr>
			</template>
		
			<template for="file">
				<td data="preview"></td>
				<td><b data="name"></b></td> <!-- note: bold tag gets data -->
				<td data="size"></td>
				<td data="delete"></td>
			</template>
		</table>
	</div>
</div>
<!-- TODO: upload and scaffolding scripts here! -->s
</body> 
</html>