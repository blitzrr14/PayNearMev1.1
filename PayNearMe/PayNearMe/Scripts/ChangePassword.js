$(document).ready(function () {
    var cPass = false;

    var newPass = false;
    var cPassIndicator = false;
    var confirmNPass = false;

    $('input#newPassword').focusout(function (e) {
        var currInput = $(this).val();
        upperLetter = false;
        smallLetter = false;
        hasNumber = false;
        cPassIndicator = false;

        if (currInput == "")
            inputInvalid('newPassword', 'msgnewPassError', 'Password field is required');
        else if (currInput.length < 8)
            inputInvalid('newPassword', 'msgnewPassError', 'Password should be atleast 8 characters long');
        else {
            for (var count = 0; (count < currInput.length && (!smallLetter || !hasNumber || !upperLetter)) ; count++) {
                if (!isNaN(currInput[count]))
                {
            
                    hasNumber = true;
                }
                else if (currInput[count] == currInput[count].toUpperCase())
                { smallLetter = true; }
                else if (currInput[count] == currInput[count].toLowerCase())
                { upperLetter = true; }
            }

            if (smallLetter && hasNumber && upperLetter) {
                inputValid('newPassword', 'msgnewPassError');
                document.getElementById("confirmPassword").focus();
                cPassIndicator = true;
                newPass = true;
            }
            else {
                cPassIndicator = false;
                password = false;
                inputInvalid('newPassword', 'msgnewPassError', 'Password must contain atleast 1 lowercase letter, 1 uppercase letter & 1 digit number');
            }
        }

        if (cPassIndicator) {
            if ($('input#confirmPassword').val() != $('input#newPassword').val()) {
                inputInvalid('confirmPassword', 'msgConfNPassError', 'Password mismatch');
            }
            else {
                inputValid('confirmPassword', 'msgConfNPassError');
            }
        }
    });
    $('input#confirmPassword').keyup(function (e) {
        if (cPassIndicator) {
            if (($(this).val() != $('input#newPassword').val()) || $(this).val() == "" || $('input#newPassword').val() == "") {
                confirmNPass = false;
                inputInvalid('confirmPassword', 'msgConfNPassError', 'Password mismatch');
            }
            else {
                confirmNPass = true;
                inputValid('confirmPassword', 'msgConfNPassError');
                $('span.msgPassError').removeClass('glyphicon-remove');
                $('span.msgPassError').removeClass('glyphicon-ok');
            }
        }
    });
    $("#changepassword-form").on('submit', function (e) {
        if (!confirmNPass)
            e.preventDefault();
    });

});