USE ieltsDb;

CREATE TABLE Role(
    role_id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100)
);

CREATE TABLE User(
    user_id INT PRIMARY KEY AUTO_INCREMENT,
    email VARCHAR(255) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    role_id INT DEFAULT 1,
    avatar_link VARCHAR(255),
    goal DECIMAL(2,1) DEFAULT 5.0 CHECK(goal >= 1.0 AND goal <= 9.0),
    isActive TINYINT(1) DEFAULT 1,
    FOREIGN KEY (role_id) REFERENCES Role(role_id)
);

CREATE TABLE Sound(
    sound_id INT PRIMARY KEY AUTO_INCREMENT,
    sound_link VARCHAR(255) NOT NULL
);

CREATE TABLE Test(
    test_id INT PRIMARY KEY AUTO_INCREMENT,
    test_type ENUM('reading','listening') NOT NULL,
    name VARCHAR(255) NOT NULL,
    year_edition INT NOT NULL,
    user_completed_num INT DEFAULT 0,
    sound_id INT,
    FOREIGN KEY (sound_id) REFERENCES Sound(sound_id)
);

CREATE TABLE ReadingSection(
    rsection_id INT PRIMARY KEY AUTO_INCREMENT,
    image_link VARCHAR(255),
    title VARCHAR(255) NOT NULL,
    content VARCHAR(10000) NOT NULL
);

CREATE TABLE Test_ReadingSection(
    test_id INT NOT NULL,
    rsection_id INT NOT NULL,
    PRIMARY KEY (test_id, rsection_id),
    FOREIGN KEY (test_id) REFERENCES Test(test_id),
    FOREIGN KEY (rsection_id) REFERENCES ReadingSection(rsection_id) 
);

CREATE TABLE ListeningSection(
    lsection_id INT PRIMARY KEY AUTO_INCREMENT,
    section_order INT NOT NULL,
    time_stamp TIME NOT NULL,
    transcript VARCHAR(10000),
    sound_id INT NOT NULL,
    FOREIGN KEY(sound_id) REFERENCES Sound(sound_id)
);

CREATE TABLE QuestionList(
    qlist_id INT PRIMARY KEY AUTO_INCREMENT,
    qlist_type ENUM('multiple_choice','matching','true_false','complete','diagram') NOT NULL,
    content VARCHAR(1000) NOT NULL,
    qnum INT NOT NULL,
    section_id INT NOT NULL,
    section_type ENUM('reading','listening') NOT NULL
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
    user_id INT NOT NULL,
    test_id INT NOT NULL,
    date_make DATETIME DEFAULT CURRENT_TIMESTAMP,
    complete_time TIME,
    FOREIGN KEY (user_id) REFERENCES User(user_id),
    FOREIGN KEY (test_id) REFERENCES Test(test_id)
);

CREATE TABLE ResultDetails(
    detail_id INT PRIMARY KEY AUTO_INCREMENT,
    result_id INT NOT NULL,
    order INT NOT NULL,
    question_id INT NOT NULL,
    user_answer VARCHAR(1000) NOT NULL,
    question_state ENUM('right','wrong') NOT NULL,
    FOREIGN KEY (result_id) REFERENCES Result(result_id),
    FOREIGN KEY (question_id) REFERENCES Question(question_id)
);

CREATE TABLE Constant(
    name VARCHAR(255) PRIMARY KEY,
    value DECIMAL(10,2) NOT NULL
);