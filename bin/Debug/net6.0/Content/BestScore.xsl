<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" encoding="UTF-8" indent="yes"/>

	<xsl:template match="/">
		<html>
			<head>
				<META http-equiv="Content-Type" content="text/html; charset=utf-8"/>
				<title>Classement des meilleurs scores</title>
				<style>
					body {
					font-family: Arial, sans-serif;
					background-color: #f4f4f9;
					color: #333;
					margin: 0;
					padding: 20px;
					}
					h1 {
					text-align: center;
					color: #444;
					margin-bottom: 30px;
					border-bottom: 2px solid #ddd;
					padding-bottom: 10px;
					}
					table {
					width: 100%;
					border-collapse: collapse;
					background-color: white;
					box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
					border-radius: 5px;
					overflow: hidden;
					}
					th {
					background-color: #e7f3ff;
					padding: 12px;
					text-align: left;
					font-weight: bold;
					color: #444;
					}
					td {
					padding: 10px;
					border-top: 1px solid #ddd;
					}
					tr:hover {
					background-color: #f5f5f5;
					}
				</style>
			</head>
			<body>
				<h1>Classement des meilleurs scores</h1>
				<table>
					<tr>
						<th>Joueur</th>
						<th>Meilleur Score</th>
					</tr>
					<xsl:for-each select="GameContent/Player">
						<xsl:sort select="scores/Score[not(. &lt; ../Score)]" data-type="number" order="descending"/>
						<tr>
							<td>
								<xsl:value-of select="Name"/>
							</td>
							<td>
								<xsl:for-each select="scores/Score">
									<xsl:sort select="." data-type="number" order="descending"/>
									<xsl:if test="position() = 1">
										<xsl:value-of select="."/>
									</xsl:if>
								</xsl:for-each>
							</td>
						</tr>
					</xsl:for-each>
				</table>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>