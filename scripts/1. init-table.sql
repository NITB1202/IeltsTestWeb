USE ieltsDb;

CREATE TABLE Constant(
    name VARCHAR(255) PRIMARY KEY,
    value DECIMAL(10,2) NOT NULL
);

CREATE TABLE Role(
    role_id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100)
);

CREATE TABLE Account(
    account_id INT PRIMARY KEY AUTO_INCREMENT,
    email VARCHAR(255) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    role_id INT DEFAULT 1,
    avatar_link VARCHAR(255),
    goal DECIMAL(2,1) DEFAULT 5.0 CHECK(goal >= 1.0 AND goal <= 9.0),
    isActive TINYINT(1) DEFAULT 1,
    FOREIGN KEY (role_id) REFERENCES Role(role_id)
);

CREATE TABLE Test(
    test_id INT PRIMARY KEY AUTO_INCREMENT,
    test_type ENUM('general','academic') NOT NULL,
    test_skill ENUM('reading','listening') NOT NULL,
    name VARCHAR(255) NOT NULL,
    month_edition INT NOT NULL CHECK(month_edition >=1 AND month_edition <=12), 
    year_edition INT NOT NULL CHECK(year_edition > 0),
    user_completed_num INT DEFAULT 0
);

CREATE TABLE Sound(
    sound_id INT PRIMARY KEY AUTO_INCREMENT,
    sound_link VARCHAR(255) NOT NULL,
    test_id INT NOT NULL,
    FOREIGN KEY (test_id) REFERENCES Test(test_id)
);

CREATE TABLE ListeningSection(
    lsection_id INT PRIMARY KEY AUTO_INCREMENT,
    section_order INT NOT NULL,
    time_stamp TIME NOT NULL,
    transcript VARCHAR(10000),
    sound_id INT NOT NULL,
    FOREIGN KEY(sound_id) REFERENCES Sound(sound_id)
);

CREATE TABLE ReadingSection(
    rsection_id INT PRIMARY KEY AUTO_INCREMENT,
    image_link VARCHAR(255),
    title VARCHAR(255) NOT NULL,
    content VARCHAR(10000) NOT NULL,
    test_id INT NOT NULL,
    FOREIGN KEY (test_id) REFERENCES Test(test_id)
);

CREATE TABLE QuestionList(
    qlist_id INT PRIMARY KEY AUTO_INCREMENT,
    qlist_type ENUM('multiple_choice','matching','true_false','complete','diagram') NOT NULL,
    content VARCHAR(1000), 
    qnum INT NOT NULL
);

CREATE TABLE DiagramQuestionList(
    dqlist_id INT PRIMARY KEY AUTO_INCREMENT,
    qlist_id INT NOT NULL,
    image_link VARCHAR(255) NOT NULL,
    FOREIGN KEY (qlist_id) REFERENCES QuestionList(qlist_id)
);

CREATE TABLE MatchQuestionList(
    mqlist_id INT PRIMARY KEY AUTO_INCREMENT,
    qlist_id INT NOT NULL,
    choice_list VARCHAR(255) NOT NULL,
    FOREIGN KEY (qlist_id) REFERENCES QuestionList(qlist_id)
);

CREATE TABLE ListeningSection_QuestionList(
    lsection_id INT NOT NULL,
    qlist_id INT NOT NULL,
    PRIMARY KEY (lsection_id,qlist_id),
    FOREIGN KEY (lsection_id) REFERENCES ListeningSection(lsection_id),
    FOREIGN KEY (qlist_id) REFERENCES QuestionList(qlist_id)
);

CREATE TABLE ReadingSection_QuestionList(
    rsection_id INT NOT NULL,
    qlist_id INT NOT NULL,
    PRIMARY KEY (rsection_id,qlist_id),
    FOREIGN KEY (rsection_id) REFERENCES ReadingSection(rsection_id),
    FOREIGN KEY (qlist_id) REFERENCES QuestionList(qlist_id)
);

CREATE TABLE Question(
    question_id INT PRIMARY KEY AUTO_INCREMENT,
    qlist_id INT NOT NULL,
    content VARCHAR(1000),
    choice_list VARCHAR(1000),
    answer VARCHAR(1000) NOT NULL
);

CREATE TABLE Explanation(
    ex_id INT PRIMARY KEY AUTO_INCREMENT,
    content VARCHAR(1000) NOT NULL,
    question_id INT NOT NULL,
    FOREIGN KEY (question_id) REFERENCES Question(question_id)
);

CREATE TABLE Result(
    result_id INT PRIMARY KEY AUTO_INCREMENT,
    score INT DEFAULT 0,
    account_id INT NOT NULL,
    test_id INT NOT NULL,
    test_access ENUM('public','private') NOT NULL,
    date_make DATETIME DEFAULT CURRENT_TIMESTAMP,
    complete_time TIME,
    FOREIGN KEY (account_id) REFERENCES Account(account_id)
);

CREATE TABLE ResultDetails(
    detail_id INT PRIMARY KEY AUTO_INCREMENT,
    result_id INT NOT NULL,
    question_order INT NOT NULL,
    question_id INT NOT NULL,
    user_answer VARCHAR(1000) NOT NULL,
    question_state ENUM('right','wrong') NOT NULL,
    FOREIGN KEY (result_id) REFERENCES Result(result_id),
    FOREIGN KEY (question_id) REFERENCES Question(question_id)
);

CREATE TABLE UserTest(
    utest_id INT PRIMARY KEY AUTO_INCREMENT,
    account_id INT NOT NULL,
    name VARCHAR(255) NOT NULL,
    date_create DATETIME DEFAULT CURRENT_TIMESTAMP,
    test_type ENUM('general','academic') NOT NULL,
    test_skill ENUM('listening', 'reading') NOT NULL,
    FOREIGN KEY (account_id) REFERENCES Account(account_id)
);

CREATE TABLE UserTestDetails(
    tdetail_id INT PRIMARY KEY AUTO_INCREMENT,
    utest_id INT NOT NULL,
    section_id INT NOT NULL,
    FOREIGN KEY (utest_id) REFERENCES UserTest(utest_id)
);