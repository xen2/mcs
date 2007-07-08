Sys.Application.add_load(SetButton);
function SetButton()
{
    $get('Button1').value = Answer.Verify;
}
function CheckAnswer()
{
    var firstInt = $get('firstNumber').innerText;
    var secondInt = $get('secondNumber').innerText;
    var userAnswer = $get('userAnswer');
    
    if ((Number.parseLocale(firstInt) + Number.parseLocale(secondInt)) == userAnswer.value)
    {
        alert(Answer.Correct);
        return true;
    }
    else
    {
        alert(Answer.Incorrect);
        return false;
    }
}
Answer={
"Verify":"Verificare la risposta",
"Correct":"Si, la risposta e’ corretta.",
"Incorrect":"No, la risposta e’ sbagliata."
};
