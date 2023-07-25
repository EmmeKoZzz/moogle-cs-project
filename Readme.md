# Moogle

## Requisitos

- .NET Framework

## Ejecución y utilización

Para ejecutar el proyecto, primero agregue los documentos que serán tratados como base de datos para las búsquedas en la carpeta "Content", luego ubique la terminal en la carpeta scrpt y haga ejecutable el script con el comando `chmod +x proyecto.sh`. Una vez el script sea ejecutable será capaz de usar las siguientes funciones:

- **`./proyecto.sh run`**: Ejecuta el proyecto.
- **`./proyecto.sh report`**: Compila el informe.
- **`./proyecto.sh slides`**: Compila la presentación.
- **`./proyecto.sh show_report`**: Ejecuta el PDF del informe.
- **`./proyecto.sh show_slides`**: Ejecuta el PDF de la presentación.
- **`./proyecto.sh clean`**: Elimina todos los archivos creados por el debug de .Net o la compilacion de los PDF por LATEX.

Una vez el proyecto ejecutado, si el navegador no se abre, entonces, puede acceder a él mediante la dirección que se muestra en la terminal. Una vez en el navegador, solo necesita escribir, en la entrada de texto que se le muestra, la búsqueda que desee realizar entre los documentos antes mencionados.

## Características

### Operadores

El buscador soporta varios operadores:

- **Operador de importancia (*)**:
Se escribe justo delante de la palabra que a la que se le quiere otorgar importancia en la búsqueda. A mayor relevancia tenga esta palabra en el documento más relevante este será.
- **Operador de necesidad (^) y de no-aparición (!)**:
Se escribe justo delante de la palabra y, en dependencia del operador, indica si esa palabra, tiene que aparecer en los documentos mostrados, en caso del operador de necesidad, o que la palabra no puede aparecer en los resultados en caso del operador de no-aparición.
- **Operador de cercanía(~)**:
Se escribe entre las palabras que se desea que estén lo más cercanas posible. A mayor cercanía entre estas palabras más relevante será el documento.

### Funcionalidades

- El buscador te dará sugerencias que te mostrarán más resultados o que te ayudarán a corregir algún error en la consulta.

- En los resultados te mostrará una porción del documento donde se intentará mostrar la parte del documento con mayor relevancia en orden de mejorar su búsqueda
