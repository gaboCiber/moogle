#!/bin/bash

# Funcion destinada a borrar los archivos temporales que se crean durante la compilacion de un archivo latex
RemoveLatexTemp(){
	for file in `find $1 -maxdepth 1 ! -name '*.tex'`
	do
		rm $file 2> /dev/null
	done
}

# Funcion destinada a borrar los archivos temporales que se crean durante la compilacion del proyecto
RemoveProyectTemp(){
	rm -r $1/bin/ 2> /dev/null
	rm -r $1/obj/ 2> /dev/null
}

# Funcion destinada a comprobar si un comando esta instalado en la maquina
ComprobarComando(){
		path=$(which $1)

		if [ "$path" != "" ]
		then
			return 0 
		else
			return 1
		fi	
}

# Funcion destinada a compilar un fichero latex y generar su pdf
# $1: el fichero latex
# $2: el comando que va compilar el fichero latex
CompilarPDF(){
	if [ "$2" = "" ]
	then
		if [ $(ComprobarComando pdflatex)$? -eq 0 ]
		then
			echo "Compilando $1 a través de pdflatex ..."
			pdflatex $1 > /dev/null
		else
			echo -e "\e[31mError: El comando pdflatex no está instalado\e[0m"
			echo "Instale pdflatex o especifíque uno alternativo como argumento"
		fi	
		 
	else
		if [ $(ComprobarComando $2)$? -eq 0 ]
		then
			echo "Compilando $1 a través de $2 ..."
			$2 $1 > /dev/null
		else
			echo -e "\e[31mError: El comando $2 no está instalado\e[0m"
		fi	
	fi
}

# Funcion destinada a abrir un fichero pdf en una ruta determinada
# $1: la ruta del fichero
# $2: el comando que va abrir el fichero
AbrirPDF(){
	if [ "$2" = "" ]
	then
		xdg-open $1 
	else
		if [ $(ComprobarComando $2)$? -eq 0 ]
		then
			$2 $1 
		else
			echo -e "\e[31mError: El comando $2 no está instalado\e[0m"
		fi	
	fi
}

# Funcion destinada a mostrar los comandos disponibles para utilizar el script
MostrarComandos(){
	echo "Comandos:"
	echo " $0                        \"Mostar el menu\""
	echo " $0 run                    \"Ejecutar el proyecto\""
	echo " $0 report                 \"Compilar y generar el pdf del latex relativo al informe con pdflatex\" "
	echo " $0 report [comando]       \"Compilar y generar el pdf del latex relativo al informe con el comando especificado\" "
	echo " $0 slides                 \"Compilar y generar el pdf del latex relativo a la presentación con pdflatex\" "
	echo " $0 slides [comando]       \"Compilar y generar el pdf del latex relativo a la presentación con el comando especificado\" "
	echo " $0 show_report            \"Visualizar el pdf del informe con el lector de PDF predeterminado\""
	echo " $0 show_report [comando]  \"Visualizar el pdf del informe con el comando especificado\""
	echo " $0 show_slides            \"Visualizar el pdf de la presentación\""
	echo " $0 show_slides [comando]  \"Visualizar el pdf de la presentación con el comando especificado\""
	echo " $0 clean                  \"Eliminar los archivos temporales de la compilación del proyecto, el informe y la presentación\" "
	echo ""
}


# Funcion destinada a evaluar los argumentos del script
Evaluar(){
	case $1 in
		run)
			xdg-open http://localhost:5000/ > /dev/null 2> /dev/null 
			dotnet watch run --project ../MoogleServer 
			;;
		report)
			cd ../Informe
			CompilarPDF informe.tex $2
			;;
		slides)
			cd ../Presentacion
			CompilarPDF presentacion.tex $2
			;;
		show_report)
			if ! [ -e ../Informe/informe.pdf ]
			then 
				Evaluar report
			fi

			AbrirPDF ../Informe/informe.pdf $2 
			;;
		show_slides)
			if ! [ -e ../Presentacion/presentacion.pdf ]
			then 
				Evaluar slides 
			fi

			AbrirPDF ../Presentacion/presentacion.pdf $2
			;;
		clean)
			RemoveLatexTemp ../Informe
			RemoveLatexTemp ../Presentacion
			RemoveLatexTemp ../Presentacion/sections
			RemoveProyectTemp ../MoogleServer
			RemoveProyectTemp ../MoogleEngine
			;;
		*) 
			echo -e "\e[31mError: Argumento inválido\e[0m"
			;;
	esac
}

# Funcion destinada a mostrar las opciones del menu
MostrarOpciones(){
		echo "Opciones:"
		echo "1) \"Ejecutar el proyecto\""
		echo "2) \"Compilar y generar el pdf del proyecto latex relativo al informe\" "
		echo "3) \"Compilar y generar el pdf del proyecto latex relativo a la presentación\" "
		echo "4) \"Visualizar el pdf del informe\""
		echo "5) \"Visualizar el pdf de la presentación\""
		echo "6) \"Eliminar los archivos temporales creados durante la compilación del proyecto, el informe y la presentación\" "
		echo "7) \"Utilizar la línea de comandos\""
		echo "8) \"Info\" "
		echo "9) \"Salir\""
		echo ""
}

MostrarInfo(){
		echo "-------------------------------------------------------"
		echo "| Proyecto desarrollado por Gabirel Andrés Pla Lasa   |"
		echo "| 1er año de Ciencia de la Computación                |"
		echo "| Facultad de Matemática y Computación                |"  
		echo "| Universidad de La Habana                            |"
		echo "-------------------------------------------------------"
}

# Funcion destinada a ejecutar el menu
Inicio(){
	
	clear

	printf "%*s\n" $(( ($(tput cols) - ${#TEXTO}) / 2)) "Proyecto Moogle"
	echo ""

	MostrarOpciones
	
	echo -n "Entre la opcion: "
	read -n 1 opcion
	echo ""
	echo ""
	
	case $opcion in
		"1")
			echo "Ejecutando el proyecto"
			Evaluar run
			Inicio
			;;
		"2")
			Evaluar report
			Inicio
			;;
		"3")
			Evaluar slides
			Inicio
			;;
		"4")
			Evaluar show_report 
			Inicio
			;;
		"5")
			Evaluar show_slides 
			Inicio
			;;
		"6")
			Evaluar clean 
			echo "Los archivos temporales fueron eliminados"
			sleep 2
			Inicio
			;;
		"7")
			MostrarComandos
			exit 0
			;;
		"8")
			MostrarInfo
			echo "" 
			exit 0
			;;
		"9")
			exit 0
			;;
		*)
			#echo -e "\e[31mError: Opción inválida\e[0m"
			#sleep 1 
			Inicio
			;;
	esac
}

# Funcion destinada a comprobar el numero de argumentos del script
ComprobarArgumentos(){
	if [ $# -eq 0 ]
	then 
		Inicio
	elif [ $# -ne 1 ] && ! [[ ( "$1" = "slides" || "$1" = "report" || "$1" = "show_slides" || "$1" = "show_report" ) &&  $# -eq 2 ]]
	then
		echo -e "\e[31mError: El número de argumentos es incorrecto\e[0m"
	else
		Evaluar $1 $2
	fi
}


ComprobarArgumentos $*
