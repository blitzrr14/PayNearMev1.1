$(document).ready(function () {
    var bdate = false;
    var sendingAmount = false;
    var beneId = false;
    var recipientReceivestextId = false;


    //$('#sendsend').click(function () {
    //    $('#confirmationMainDivId').slideUp();
    //    $('#confirmReceiptMainDivId').slideDown();
    //});

    $('#beneficiaryform').click(function () {
        $('#defaultbeneficiary').hide();
        $('#add_newbeneficiarymain').show();
    });

    $('#cancelAddBenef').click(function () {
        $('#defaultbeneficiary').show();
        $('#addbeneficiaryform').hide();
    });

    $('#continue1').click(function () {
      
        if ($('#sendingAmount').val() == 0) {
            errModal("Send Amount is Empty");
            return;
        }
        else if ($('#recipientReceivestextId').val() == 0) {
            errModal("Send Amount is Empty");
            return;
        }
        else if ($('#sendingAmount').val() < 1) {
            errModal("Send Amount Must 1 USD Above");
            return;
        }
        else if ($('#sendingAmount').val() > 1000) {
            errModal("Send Amount Must 1000 USD Below");
            return;
        }
        else if ($('#beneId').val().toString() == '') {
            errModal("Select Beneficiary");
            return;
        }

        var receiverCustId = $('#beneId option:selected').val();
        $('#confirmSendingAmount').val($('#sendingAmount').val());

        ajxloadtoggle();
        $.ajax({
            url: '../api/WebService/json/getBeneficiaryInfo/?receiverCustID='+receiverCustId +'',
            type: 'GET',
            success: function (resp) {
                ajxloadtoggle();
                if (resp != null)
                {
                   
                    var fName = resp.data.firstName + " " + resp.data.lastname;
                    var address = resp.data.city + " " + resp.data.country;
                    var total = parseFloat($('#sendingAmount').val()) + parseFloat($('#serviceFee').val());
                    var amountpo = parseFloat($('#sendingAmount').val()) * parseFloat($('#exchangeRate').val());
                    $('#beneNameStep3').val(fName.toUpperCase());
                    $('#beneAddressStep3').val(address.toUpperCase());
                    $('#benePhoneStep3').val(resp.data.phoneNo);
                    $('#confirmSendingAmount').text($('#sendingAmount').val());
                    $('#confirmSendingTotal').val(total.toFixed(2));
                    $('#receiverCustID').val(receiverCustId);
                    $('#confirmSendingCharge').val($('#serviceFee').text());
                    $('#amountPO').val(amountpo.toFixed(2))

              
                    $('#beneficiarybody').slideUp();
                    $('#confirmationbody').slideDown();

                    //promocode----------------------------------------------------
                    $("#Discount").val("0.0");
                    $("#NetTransferFee").val("0.0");
                    $("#TotalAfterDiscount").val("0.0");
                    $(".promoCodeTrue").hide();
                    $("#confirmPromoCode").val('').attr('readonly', false).focus();
                    $("#btnRemovePC").hide();
                    $("#btnValidatePC").show();
                    //-------------------------------------------------------------




                    
                }
               
            },
            error: function (response) {
                ajxloadtoggle();
                errModal("There was a problem on your request, please try again! Thank You!");
            }
        });
       
    });

    $('#beneId').focusout(function () {
        if ($('#beneId').val() == "") {
            $('#beneId').css({ "border-color": "#ff000a", "box-shadow": "none" });
            beneId = false;
        }
        else {
            $('#beneId').css("border-color", "#ccc");
            beneId = true;
        }
    });

    $('#beneId').change(function () {
        $('#beneName').text($('#beneId :selected').text().toUpperCase());


    });

    $('input#sendingAmount').focusout(function () {
        var usd = " USD";
        var php = " PHP";
        var res = 0;
        var bcode = "711";
        var zcode = "3";
        var sendAmount = isNaN(parseFloat($('#sendingAmount').val())) ? 0.00 : parseFloat($('#sendingAmount').val());

        if (sendAmount > 1000)
        {
            errModal("Maximum Order Amount Exceeded, Please Input not more than 1000 USD");
            return;
        }
        else if (sendAmount < 1) {
            errModal("Send Amount Must 1 USD Above");
            return;
        }
        var urx = '../Send/getCharge/';
        ajxloadtoggle();
        $.ajax({
            url: urx + '/?amount='+sendAmount + '&bcode=' + bcode + '&zcode=' + zcode + '',
            type: 'GET',
            success: function (resp) {
                ajxloadtoggle();
                if (resp.respcode == 1) {
                    var charge = resp.charge;
                    $("#serviceFee").text(charge.toFixed(2));
                    $("#serviceFee").val(charge.toFixed(2));
                    $('#recipientReceivestextId').val((sendAmount * parseFloat($('#exchangeRate1').text())).toFixed(2));
                    $('#sendtext').text(sendAmount);
                    $('#youRSendStep2').text($('#sendingAmount').val().concat(usd));
                    $('#recieve').text($('#recipientReceivestextId').val().concat(php));
                    $('#recieve2').text($('#recipientReceivestextId').val().concat(php));
                    $('#recivingAmtStep3').text($('#recipientReceivestextId').val().concat(php));
                    $('#totAmount').text((sendAmount + charge).toFixed(2));
                }
                else
                {
                   
                    errModal(resp.message);
                }

            },
            error: function (response) {
                ajxloadtoggle();
                errModal("There was a problem on your request, please try again! Thank You!");
            }
        });


    });

    $('input#dateOfBirth').keydown(function (e) {
        var currentInput = $(this).val();
        if ((!isNumber(e) && !isBackSpace(e) && !isArrowKeys(e) && !isTab(e) && !isDelete(e) && !isSelectAll(e))
            || ((e.keyCode == 48 || e.keyCode == 96) && currentInput.length == 1 && currentInput[0] == '0')
            || currentInput.length > 10)
            e.preventDefault();
        else if ((currentInput.length == 2 || currentInput.length == 5) && !isBackSpace(e))
            $('input#dateOfBirth').val(currentInput + '/');
    });

    $('input#dateOfBirth').focusout(function (e) {
        var bDateVal = $(this).val();
        bdate = false;
        if (bDateVal != "") {
            if (bDateVal[2] != '/' || bDateVal[5] != '/') {
                $(this).val('');
                $(this).focus();
                inputInvalid('dateOfBirth', 'msgBDateError', 'Invalid date format');
            }
            else if (bDateVal.length < 10 ||
                parseInt(bDateVal.slice(0, 2)) > 12 || //if month > 12
                parseInt(bDateVal.slice(3, 5)) < 01 || //if days < 01
                parseInt(bDateVal.slice(3, 5)) > 31    //if days > 31
               ) {
                inputInvalid('dateOfBirth', 'msgBDateError', 'Invalid date format');
            }
            else if (parseInt((new Date().getFullYear()) -
                     parseInt(bDateVal.slice(6))) < 0)
            { inputInvalid('dateOfBirth', 'msgBDateError', 'Not born yet?'); }
            else if (parseInt((new Date().getFullYear()) -
                     parseInt(bDateVal.slice(6))) > 200)
            { inputInvalid('dateOfBirth', 'msgBDateError', 'Is this correct?'); }
            else {
                inputValid('dateOfBirth', 'msgBDateError');
                bdate = true;
            }
        }
    });

    $('input#phoneNo').keydown(function (e) {
        var currentInput = $(this).val();
        if ((!isNumber(e) && !isBackSpace(e) && !isArrowKeys(e) && !isTab(e) && !isDelete(e) && !isSelectAll(e))
            || currentInput.length > 10)
            e.preventDefault();
    });

    $('input#phoneNo').focusout(function (e) {
        if ($(this).val().length < 10) {
            $('.msgMobile').css("border-color", "#ff000a");
            $('div#isdCodeId').css("color", "#ff000a");
            $('div#isdCodeId').css("background-color", "#fee");
            mobile = false;
        }
        else {
            $('.msgMobile').css("border-color", "green");
            $('div#isdCodeId').css("color", "black");
            $('div#isdCodeId').css("background-color", "#e5ffe5");
            mobile = true;
        }
    });

    $('input#zipCode').keydown(function (e) {
        if ((!isNumber(e) && !isBackSpace(e) && !isArrowKeys(e) && !isTab(e) && !isDelete(e) && !isSelectAll(e))
            || e.length > 9)
            e.preventDefault();
    });

    $('#relation').focusout(function () {
        if ($('#relation').val() == "") {
            $('#relation').css({ "border-color": "#ff000a", "box-shadow": "none" });
            phoneNo = false;
        }
        else {
            $('#relation').css({ "border-color": "#ccc" });
            phoneNo = true;
        }
    });



});

//---------- SENDOUT JS ----------------

    $(document).ready(function () {

        $("#beneficiaryformR").click(function () {
            ajxloadtoggle();
            var uri = $("#beneficiaryformR").attr("data-addbene");
            $.ajax({
                url: uri,
                type: 'POST',
                success: function (data) {

                    $("#addBenficiaryPartial").html(data);
                    $("#addBenficiaryPartial").css("margin-top", "-70px");
                    $("#BeneficiaryMainId").hide();
                    ajxloadtoggle();
                },
                error: function (response) {
                    errModal("There was a problem on your request, please try again! Thank You!");
                    ajxloadtoggle();
                }
            });
        });

        $("#sendoutOrderConfirm").click(function () {
            ajxloadtoggle();
            var model = $("#SendOrder").serialize();
            var uri = $("#sendoutOrderConfirm").attr("data-submit");
            $.ajax({
                url: uri,
                type: 'POST',
                data: model,
                success: function (data) {

                    if (data.respcode == "ok") {
                        successModal(data.trackingUrl,data.amount);
                    }
                    else
                    {
                        failModal(data.message);

                    }
                    ajxloadtoggle();

                },
                error: function (response) {
                    failModal("There was a problem on your request, please try again! Thank You!");
                    ajxloadtoggle();
                }
            });
        });

        $("#editSendout").click(function ()
        {
            $('#beneficiarybody').slideDown();
            $('#confirmationbody').slideUp();
        });

        $("#btnValidatePC").click(function ()
        {
            var promoCode = $("#confirmPromoCode").val();
            var charge = $("#confirmSendingCharge").val();
            var amount = $("#confirmSendingAmount").val();
            var user = "wsuser";
            var pass = "wspasswordRrykuqt14!";
            if (promoCode == '' || promoCode == null)
            {
                return errModal("Please input promocode.");
            }

            ajxloadtoggle();
            var uri = $("#btnValidatePC").attr("data-validate");
            var form = $('#SendOrder');
            var token = $('input[name="__RequestVerificationToken"]', form).val();
            var model = { __RequestVerificationToken: token,Username: user, Password: pass, promoCode: promoCode, Charge: charge, Amount: amount };

            $.ajax({
                url: uri,
                type: 'POST',
                data: model,
                success: function (resp) {

                    var respx = JSON.parse(resp);
                    if (respx.respcode == 1) {

                        $("#Discount").val(respx.discount.toFixed("2"));
                        $("#NetTransferFee").val(respx.newCharge.toFixed("2"));
                        $("#TotalAfterDiscount").val(respx.newTotal.toFixed("2"));
                        $(".promoCodeTrue").show();
                        $("#btnRemovePC").show();
                        $("#btnValidatePC").css('display', 'none');
                        $("#confirmPromoCode").attr('readonly', true);


                    }
                    else {
                        errModal(respx.message);

                    }
                    ajxloadtoggle();

                },
                error: function (resp) {
                    errModal("There was a problem on your request, please try again! Thank You!");
                    ajxloadtoggle();
                }
            });

        });

        $("#btnRemovePC").click(function () {
            ajxloadtoggle();
            var form = $('#SendOrder');
            var token = $('input[name="__RequestVerificationToken"]', form).val();
            var model = { __RequestVerificationToken: token, Username: 'wsuser', Password: 'wspasswordRrykuqt14!', promoCode: $("#confirmPromoCode").val() };
          //  var model = {  Username: 'wsuser', Password: 'wspasswordRrykuqt14!', promoCode: $("#confirmPromoCode").val() };

            var uri = $("#btnRemovePC").attr("data-remove");

            $.ajax({
                url: uri,
                type: 'POST',
                data: model,
                success: function (resp) {

                    var respx = JSON.parse(resp);

                    if (respx.respcode == 1) {

                        $("#Discount").val("0.0");
                        $("#NetTransferFee").val("0.0");
                        $("#TotalAfterDiscount").val("0.0");
                        $(".promoCodeTrue").hide();
                        $("#confirmPromoCode").val('').attr('readonly', false).focus();
                        $("#btnRemovePC").hide();
                        $("#btnValidatePC").show();


                    }
                    else {
                        errModal(respx.message);

                    }
                    ajxloadtoggle();

                },
                error: function (resp) {
                    errModal("There was a problem on your request, please try again! Thank You!");
                    ajxloadtoggle();
                }
            });

        });


        $("#linkPNM").click(function () {

            var uri = $("#linkPNM").attr("data-xxsend");
            window.location.href = uri;
        });


        $('#cbPC').on('click', function () {
            if (document.getElementById('cbPC').checked == true) {
                $('.promoCodeUI').removeClass('pcuiHide');
                $('.promoCodeUI').addClass('pcuiShow');

                //promocode----------------------------------------------------
                $("#Discount").val("0.0");
                $("#NetTransferFee").val("0.0");
                $("#TotalAfterDiscount").val("0.0");
                $(".promoCodeTrue").hide();
                $("#confirmPromoCode").val('').attr('readonly', false).focus();
                $("#btnRemovePC").hide();
                $("#btnValidatePC").show();
                //-------------------------------------------------------------

            }
            else {
                $('.promoCodeUI').removeClass('pcuiShow');
                $('.promoCodeUI').addClass('pcuiHide');

                //promocode----------------------------------------------------
                $("#Discount").val("0.0");
                $("#NetTransferFee").val("0.0");
                $("#TotalAfterDiscount").val("0.0");
                $(".promoCodeTrue").hide();
                $("#confirmPromoCode").val('').attr('readonly', false);
                $("#btnRemovePC").hide();
                $("#btnValidatePC").show();
                //-------------------------------------------------------------
            }
        })

    });
function errModal(message) {
    $("#modContent").html(message);
    $("#myModal").show();
}

function successModal(trackingURL, amount) {
    $("#linkPNM").attr("href", trackingURL);
    $("#myModalSuccess").show();
    $("#amountSuccess").text(amount);
}

function failModal(message) {
    $("#contentFail").html(message);
    $("#myModalFailed").show();
}
