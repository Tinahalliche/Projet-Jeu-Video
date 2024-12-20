<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:output method="html" encoding="UTF-8" indent="yes"/>

	<xsl:template match="/">
		<html>
			<head>
				<META http-equiv="Content-Type" content="text/html; charset=utf-8"/>
				<title>Scores des Joueurs</title>
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
					h2 {
					color: #555;
					margin-top: 20px;
					}
					p {
					font-size: 1.1em;
					color: #666;
					}
					ul {
					list-style-type: none;
					padding: 0;
					}
					ul li {
					background-color: #e7f3ff;
					margin: 5px 0;
					padding: 10px;
					border-radius: 5px;
					box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
					}
					ul li:hover {
					background-color: #cce7ff;
					}
				</style>
			</head>
			<body>
				<h1>Scores des Joueurs</h1>
				<xsl:for-each select="GameContent/Player">
					<h2>
						Joueur : <xsl:value-of select="Name"/>
					</h2>
					<p>Scores :</p>
					<ul>
						<xsl:for-each select="scores/Score">
							<xsl:sort select="." data-type="number" order="descending"/>
							<li>
								<xsl:value-of select="."/>
							</li>
						</xsl:for-each>
					</ul>
				</xsl:for-each>
			</body>
		</html>
	</xsl:template>

</xsl:stylesheet>